# language: es
Característica: Perfil de visitante
  Un usuario se convierte en visitante registrado al completar su perfil.
  El perfil se completa una sola vez y el documento es único entre visitantes.

  Antecedentes:
    Dado el usuario "Víctor" con rol Visitor
    Y el usuario "Valeria" con rol Visitor
    Y el usuario "Marta" con rol Moderador

  Escenario: Un usuario sin perfil todavía no es visitante
    Cuando "Víctor" consulta su registro de visitante
    Entonces el registro de visitante no se encuentra

  Escenario: Un visitante completa su perfil
    Cuando "Víctor" completa su perfil de visitante con:
      | nombres | apellidos   | tipo documento  | documento | correo             | teléfono   |
      | Víctor  | Rojas Peña  | CitizenshipCard | 1020304   | victor@example.com | 3109876543 |
    Entonces "Víctor" consulta su registro de visitante y ve:
      | estado     | nombre            | documento | correo             | teléfono      |
      | Registered | Víctor Rojas Peña | 1020304   | victor@example.com | +573109876543 |

  Escenario: El perfil solo se completa una vez
    Dado que "Víctor" completó su perfil de visitante con documento "1020304"
    Cuando "Víctor" completa su perfil de visitante con documento "9080706"
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "El visitante ya completó su registro."

  Escenario: No se admiten dos visitantes con el mismo documento
    Dado que "Víctor" completó su perfil de visitante con documento "1020304"
    Cuando "Valeria" completa su perfil de visitante con documento "1020304"
    Entonces la solicitud se rechaza por validación con el mensaje "Ya existe un visitante con ese documento."

  Escenario: Un documento con formato inválido se rechaza
    Cuando "Víctor" completa su perfil de visitante con documento "12AB"
    Entonces la solicitud se rechaza por regla de negocio con el mensaje "El número de documento no tiene un formato válido."

  Escenario: Un visitante no puede consultar el registro de otro
    Dado que "Víctor" completó su perfil de visitante con documento "1020304"
    Cuando "Valeria" consulta el registro de visitante de "Víctor"
    Entonces la solicitud se rechaza por permisos

  Escenario: Moderación puede consultar y listar visitantes
    Dado que "Víctor" completó su perfil de visitante con documento "1020304"
    Cuando "Marta" consulta el registro de visitante de "Víctor"
    Entonces el visitante está en estado Registered
    Cuando "Marta" lista los visitantes
    Entonces la lista contiene 1 visitante

  Escenario: Un visitante no puede listar los visitantes
    Cuando "Víctor" lista los visitantes
    Entonces la solicitud se rechaza por permisos
