const stripBom = (text: string) => text.replace(/^﻿/, '');

export function whole(source: string) {
    return stripBom(source).trimEnd();
}

export function region(source: string, name: string) {
    const start = new RegExp(`^[ \\t]*#region\\s+${name}\\s*$`, 'm');
    const end = /^[ \t]*#endregion.*$/m;

    const opened = stripBom(source).split(start)[1];

    if (opened === undefined)
        throw new Error(`No existe la región '${name}' en el archivo importado.`);

    const body = opened.split(end)[0].replace(/^\r?\n/, '').trimEnd();
    const indent = Math.min(...body.split('\n').filter(l => l.trim()).map(l => l.match(/^[ \t]*/)![0].length));

    return body.split('\n').map(l => l.slice(indent)).join('\n');
}
