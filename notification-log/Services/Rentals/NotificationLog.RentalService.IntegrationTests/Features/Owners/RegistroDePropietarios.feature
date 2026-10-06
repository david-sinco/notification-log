# language: es
Característica: Registro de propietarios
  Un propietario se registra a sí mismo, o lo registra un administrador o moderador.
  El documento y el NIT son únicos entre propietarios.

  Antecedentes:
    Dado el usuario "Ana" con rol Propietario
    Y el usuario "Pedro" con rol Propietario
    Y el usuario "Marta" con rol Moderador
    Y el usuario "Víctor" con rol Visitor

  Escenario: Un propietario se registra como persona natural
    Cuando "Ana" se registra como propietaria persona natural con:
      | nombres | apellidos    | tipo documento  | documento | correo          | teléfono   |
      | Ana     | Gómez Rincón | CitizenshipCard | 52123456  | ana@example.com | 3001234567 |
    Entonces el propietario queda registrado con el identificador de "Ana"
    Y "Ana" puede consultar ese propietario con:
      | tipo    | nombre           | documento | correo          | teléfono      |
      | Natural | Ana Gómez Rincón | 52123456  | ana@example.com | +573001234567 |
    Y se solicita una cuenta con rol Propietario para "ana@example.com"

  Escenario: Un propietario se registra como persona jurídica
    Cuando "Ana" se registra como propietaria persona jurídica con:
      | razón social       | NIT         | correo            | teléfono   |
      | Inmobiliaria Andes | 900123456-8 | andes@example.com | 6011234567 |
    Entonces el propietario queda registrado con el identificador de "Ana"
    Y "Ana" puede consultar ese propietario con:
      | tipo    | razón social       | NIT         |
      | Company | Inmobiliaria Andes | 900123456-8 |

  Escenario: Un moderador registra a un propietario en su nombre
    Cuando "Marta" registra un propietario persona natural con documento "52123456"
    Entonces el propietario queda registrado con un identificador nuevo
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
