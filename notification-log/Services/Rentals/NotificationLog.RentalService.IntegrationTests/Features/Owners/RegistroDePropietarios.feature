# language: es
Característica: Registro de propietarios
  Un propietario se registra a sí mismo, o lo registra un administrador o moderador.
  El documento y el NIT son únicos entre propietarios.
  Registrar un propietario no crea usuarios: el que registra un moderador queda sin usuario
  hasta que lo reclama quien confirmó su correo o su teléfono.

  Antecedentes:
    Dado el usuario "Ana" con rol Propietario
    Y el usuario "Pedro" con rol Propietario
    Y el usuario "Marta" con rol Moderador
    Y el usuario "Víctor" con rol Visitor

  Escenario: Un propietario se registra como persona natural
    Cuando "Ana" se registra como propietaria persona natural con:
      | nombres | apellidos    | tipo documento  | documento | correo          | teléfono   |
      | Ana     | Gómez Rincón | CitizenshipCard | 52123456  | ana@example.com | 3001234567 |
    Entonces el propietario queda registrado y relacionado con "Ana"
    Y "Ana" puede consultar ese propietario con:
      | tipo    | nombre           | documento | correo          | teléfono      |
      | Natural | Ana Gómez Rincón | 52123456  | ana@example.com | +573001234567 |

  Escenario: Un propietario se registra como persona jurídica
    Cuando "Ana" se registra como propietaria persona jurídica con:
      | razón social       | NIT         | correo            | teléfono   |
      | Inmobiliaria Andes | 900123456-8 | andes@example.com | 6011234567 |
    Entonces el propietario queda registrado y relacionado con "Ana"
    Y "Ana" puede consultar ese propietario con:
      | tipo    | razón social       | NIT         |
      | Company | Inmobiliaria Andes | 900123456-8 |

  Escenario: Un moderador registra a un propietario en su nombre
    Cuando "Marta" registra un propietario persona natural con documento "52123456"
    Entonces el propietario queda registrado con un identificador nuevo
    Y el propietario queda sin usuario relacionado
    Y "Marta" puede consultar ese propietario

  Escenario: Un propietario no puede registrarse dos veces
    Dado que "Ana" está registrada como propietaria con documento "52123456"
    Cuando "Ana" se registra como propietaria persona natural con documento "80765432"
    Entonces la solicitud se rechaza por validación con el mensaje "Ya estás registrado como propietario."

  Escenario: No se admiten dos propietarios con el mismo documento
    Dado que "Ana" está registrada como propietaria con documento "52123456"
    Cuando "Pedro" se registra como propietario persona natural con documento "52123456"
    Entonces la solicitud se rechaza por validación con el mensaje "Ya existe un propietario con ese documento."

  Escenario: No se admiten dos propietarios con el mismo NIT
    Dado que "Ana" está registrada como propietaria con NIT "900123456-8"
    Cuando "Pedro" se registra como propietario persona jurídica con NIT "900123456-8"
    Entonces la solicitud se rechaza por validación con el mensaje "Ya existe un propietario con ese NIT."

  Escenario: Un NIT con dígito de verificación incorrecto se rechaza
    Cuando "Ana" se registra como propietaria persona jurídica con NIT "900123456-1"
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "El dígito de verificación del NIT no es correcto."

  Escenario: Un visitante no puede registrar propietarios
    Cuando "Víctor" se registra como propietario persona natural con documento "52123456"
    Entonces la solicitud se rechaza por permisos

  Escenario: Un propietario no puede consultar a otro propietario
    Dado que "Ana" está registrada como propietaria con documento "52123456"
    Cuando "Pedro" consulta el propietario de "Ana"
    Entonces la solicitud se rechaza por permisos

  Escenario: Solo moderación puede listar los propietarios
    Dado que "Ana" está registrada como propietaria con documento "52123456"
    Cuando "Ana" lista los propietarios
    Entonces la solicitud se rechaza por permisos
    Cuando "Marta" lista los propietarios
    Entonces la lista contiene 1 propietario

  Escenario: El propietario aparece para reclamar a quien tiene su correo
    Dado que "Marta" registró un propietario persona natural para "Ana" con documento "52123456"
    Cuando "Ana" consulta los propietarios que puede reclamar
    Entonces la respuesta contiene solo ese propietario

  Escenario: El propietario no aparece para reclamar a quien tiene otro correo
    Dado que "Marta" registró un propietario persona natural para "Ana" con documento "52123456"
    Cuando "Pedro" consulta los propietarios que puede reclamar
    Entonces la respuesta no contiene propietarios

  Escenario: Reclamar relaciona al propietario con el usuario
    Dado que "Marta" registró un propietario persona natural para "Ana" con documento "52123456"
    Cuando "Ana" reclama ese propietario
    Entonces la solicitud se acepta
    Y el propietario queda relacionado con "Ana"
    Cuando "Ana" consulta sus propietarios
    Entonces la respuesta contiene solo ese propietario

  Escenario: Un propietario reclamado no se puede reclamar otra vez
    Dado que "Marta" registró un propietario persona natural para "Ana" con documento "52123456"
    Y que "Ana" reclamó ese propietario
    Cuando "Ana" reclama ese propietario
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "El propietario ya fue reclamado."

  Escenario: No se reclama un propietario con otro correo
    Dado que "Marta" registró un propietario persona natural para "Ana" con documento "52123456"
    Cuando "Pedro" reclama ese propietario
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "Tu correo o teléfono confirmado no coincide con el del propietario."
    Y el propietario queda sin usuario relacionado

  Escenario: Quien reclama al propietario gestiona las publicaciones que creó el moderador
    Dado que "Marta" registró un propietario persona natural para "Ana" con documento "52123456"
    Cuando "Marta" crea una publicación de arriendo a nombre de "Ana"
    Y "Ana" fija el precio en 1800000
    Entonces la solicitud se rechaza por permisos
    Cuando "Ana" reclama ese propietario
    Y "Ana" fija el precio en 1800000
    Entonces la solicitud se acepta
