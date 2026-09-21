const stripBom = (text: string) => text.replace(/^﻿/, '');

const dedent = (body: string) => {
    const kept = body.split('\n');
    const indents = kept.filter(line => line.trim()).map(line => line.match(/^[ \t]*/)![0].length);
    const indent = indents.length ? Math.min(...indents) : 0;

    return kept.map(line => line.slice(indent)).join('\n');
};

export function whole(source: string) {
    return stripBom(source).trimEnd();
}

export function region(source: string, name: string) {
    const start = new RegExp(`^[ \\t]*#region\\s+${name}\\s*$`, 'm');
    const end = /^[ \t]*#endregion.*$/m;

    const opened = stripBom(source).split(start)[1];

    if (opened === undefined)
        throw new Error(`No existe la región '${name}' en el archivo importado.`);

    return dedent(opened.split(end)[0].replace(/^\r?\n/, '').trimEnd());
}

export function lines(source: string, from: number, to: number) {
    return dedent(stripBom(source).split('\n').slice(from - 1, to).join('\n').trimEnd());
}

// Recorta desde la línea que contiene `fromText` hasta la que contiene `toText`, ambas incluidas.
// Se ancla en texto y no en números de línea para que el fragmento no se desplace al editar el archivo.
export function span(source: string, fromText: string, toText: string) {
    const all = stripBom(source).replace(/\r\n/g, '\n').split('\n');
    const from = all.findIndex(line => line.includes(fromText));

    if (from < 0)
        throw new Error(`No se encontró '${fromText}' en el archivo importado.`);

    const to = all.findIndex((line, index) => index >= from && line.includes(toText));

    if (to < 0)
        throw new Error(`No se encontró '${toText}' después de '${fromText}' en el archivo importado.`);

    return dedent(all.slice(from, to + 1).join('\n').trimEnd());
}

// Recorta un miembro de C# buscando su firma y cerrando por llaves balanceadas, ignorando
// las que aparecen dentro de cadenas y comentarios. Así una página puede citar un método
// concreto sin copiarlo: si el archivo cambia, el fragmento cambia con él o falla la compilación.
export function member(source: string, signature: string) {
    const text = stripBom(source).replace(/\r\n/g, '\n');
    const at = text.indexOf(signature);

    if (at < 0)
        throw new Error(`No se encontró '${signature}' en el archivo importado.`);

    const start = text.lastIndexOf('\n', at) + 1;

    let depth = 0;
    let parens = 0;
    let opened = false;
    let end = -1;

    for (let i = start; i < text.length; i++) {
        const char = text[i];
        const next = text[i + 1];

        if (char === '"') {
            const verbatim = text[i - 1] === '@' || text[i - 2] === '@';
            i++;
            while (i < text.length && text[i] !== '"') i += !verbatim && text[i] === '\\' ? 2 : 1;
            continue;
        }

        if (char === "'") {
            i++;
            while (i < text.length && text[i] !== "'") i += text[i] === '\\' ? 2 : 1;
            continue;
        }

        if (char === '/' && next === '/') {
            i = text.indexOf('\n', i);
            if (i < 0) break;
            continue;
        }

        if (char === '/' && next === '*') {
            i = text.indexOf('*/', i) + 1;
            continue;
        }

        if (char === '(') { parens++; continue; }
        if (char === ')') { parens--; continue; }

        // Las llaves dentro de paréntesis son inicializadores o `with` de una llamada
        // (`Raise(e with { ... });`), no el cuerpo del miembro: no cuentan para el cierre.
        if (parens > 0) continue;

        if (char === '{') { depth++; opened = true; continue; }

        if (char === '}') {
            depth--;

            if (opened && depth === 0) {
                end = i + 1;

                // Un cuerpo de expresión (`=> new { ... };`) cierra con llave y punto y coma.
                let after = end;
                while (after < text.length && /\s/.test(text[after])) after++;
                if (text[after] === ';') end = after + 1;

                break;
            }

            continue;
        }

        // Miembro de una sola sentencia (cuerpo de expresión, campo, propiedad automática).
        if (char === ';' && depth === 0) { end = i + 1; break; }
    }

    if (end < 0)
        throw new Error(`No se pudo cerrar el miembro '${signature}' en el archivo importado.`);

    return dedent(text.slice(start, end));
}

// Una sola línea, localizada por su contenido.
export function lineWith(source: string, text: string) {
    return span(source, text, text);
}

export function members(source: string, signatures: string[], separator = '\n\n') {
    return signatures.map(signature => member(source, signature)).join(separator);
}
