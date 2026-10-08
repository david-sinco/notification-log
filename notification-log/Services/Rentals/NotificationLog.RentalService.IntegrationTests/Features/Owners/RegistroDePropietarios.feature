# language: es
Característica: Registro de propietarios
  Un propietario se registra a sí mismo, o lo registra un administrador o moderador.
  Un propietario tiene un nombre, que puede ser el de una persona o el de una empresa, y un correo
  y un teléfono que lo identifican: ninguno de los dos se repite entre propietarios.
  Registrar un propietario no crea usuarios: el que registra un moderador queda sin usuario
  hasta que lo reclama quien confirmó su correo o su teléfono.

  Antecedentes:
    Dado el usuario "Ana" con rol Propietario
    Y el usuario "Pedro" con rol Propietario
    Y el usuario "Marta" con rol Moderador
    Y el usuario "Víctor" con rol Visitor

  Escenario: Un propietario se registra con su nombre y sus datos de contacto
    Cuando "Ana" se registra como propietaria con:
      | nombre             | correo            | teléfono   |
      | Inmobiliaria Andes | andes@example.com | 6011234567 |
    Entonces el propietario queda registrado y relacionado con "Ana"
    Y "Ana" puede consultar ese propietario con:
      | nombre             | correo            | teléfono      |
      | Inmobiliaria Andes | andes@example.com | +576011234567 |

  Escenario: Un moderador registra a un propietario en su nombre
    Cuando "Marta" registra un propietario
    Entonces el propietario queda registrado con un identificador nuevo
    Y el propietario queda sin usuario relacionado
    Y "Marta" puede consultar ese propietario

  Escenario: Un propietario no puede registrarse dos veces
    Dado que "Ana" está registrada como propietaria
    Cuando "Ana" se registra como propietaria
    Entonces la solicitud se rechaza por validación con el mensaje "Ya estás registrado como propietario."

  Escenario: Un propietario no puede registrar a otro propietario
    Dado que "Ana" está registrada como propietaria
    Cuando "Ana" registra un propietario
    Entonces la solicitud se rechaza por validación con el mensaje "Ya estás registrado como propietario."

  Escenario: No se admiten dos propietarios con el mismo correo
    Dado que "Ana" está registrada como propietaria con correo "ana@example.com"
    Cuando "Pedro" se registra como propietario con correo "ana@example.com"
    Entonces la solicitud se rechaza por validación con el mensaje "Ya existe un propietario con ese correo."

  Escenario: No se admiten dos propietarios con el mismo teléfono
    Dado que "Ana" está registrada como propietaria con teléfono "3001234567"
    Cuando "Pedro" se registra como propietario con teléfono "300 123 4567"
    Entonces la solicitud se rechaza por validación con el mensaje "Ya existe un propietario con ese teléfono."

  Escenario: Un teléfono con formato inválido se rechaza
    Cuando "Ana" se registra como propietaria con teléfono "12AB"
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "El teléfono no tiene un formato válido."

  Escenario: Un visitante no puede registrar propietarios
    Cuando "Víctor" se registra como propietario
    Entonces la solicitud se rechaza por permisos

  Escenario: Un propietario no puede consultar a otro propietario
    Dado que "Ana" está registrada como propietaria
    Cuando "Pedro" consulta el propietario de "Ana"
    Entonces la solicitud se rechaza por permisos

  Escenario: Solo moderación puede listar los propietarios
    Dado que "Ana" está registrada como propietaria
    Cuando "Ana" lista los propietarios
    Entonces la solicitud se rechaza por permisos
    Cuando "Marta" lista los propietarios
    Entonces la lista contiene 1 propietario

  Escenario: El propietario aparece para reclamar a quien tiene su correo
    Dado que "Marta" registró un propietario para "Ana"
    Cuando "Ana" consulta los propietarios que puede reclamar
    Entonces la respuesta contiene solo ese propietario

  Escenario: El propietario no aparece para reclamar a quien tiene otro correo
    Dado que "Marta" registró un propietario para "Ana"
    Cuando "Pedro" consulta los propietarios que puede reclamar
    Entonces la respuesta no contiene propietarios

  Escenario: Reclamar relaciona al propietario con el usuario
    Dado que "Marta" registró un propietario para "Ana"
    Cuando "Ana" reclama ese propietario
    Entonces la solicitud se acepta
    Y el propietario queda relacionado con "Ana"
    Cuando "Ana" consulta sus propietarios
    Entonces la respuesta contiene solo ese propietario

  Escenario: Un propietario reclamado no se puede reclamar otra vez
    Dado que "Marta" registró un propietario para "Ana"
    Y que "Ana" reclamó ese propietario
    Cuando "Ana" reclama ese propietario
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "El propietario ya fue reclamado."

  Escenario: No se reclama un propietario con otro correo
    Dado que "Marta" registró un propietario para "Ana"
    Cuando "Pedro" reclama ese propietario
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "Tu correo o teléfono confirmado no coincide con el del propietario."
    Y el propietario queda sin usuario relacionado

  Escenario: Quien reclama al propietario gestiona las publicaciones que creó el moderador
    Dado que "Marta" registró un propietario para "Ana"
    Cuando "Marta" crea una publicación de arriendo a nombre de "Ana"
    Y "Ana" fija el precio en 1800000
    Entonces la solicitud se rechaza por permisos
    Cuando "Ana" reclama ese propietario
    Y "Ana" fija el precio en 1800000
    Entonces la solicitud se acepta
