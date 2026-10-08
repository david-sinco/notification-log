# language: es
Característica: Negociación de una visita
  Un visitante pide una visita proponiendo de 1 a 3 franjas. El anfitrión y el visitante
  se turnan: quien tiene el turno acepta una franja o propone otras, antes de que venza el plazo.

  Antecedentes:
    Dado que ahora son las "2026-10-05 09:00" hora de Colombia
    Y el usuario "Ana" con rol Propietario
    Y el usuario "Pedro" con rol Propietario
    Y el usuario "Víctor" con rol Visitor
    Y el usuario "Marta" con rol Moderador
    Y que "Ana" está registrada como propietaria
    Y una publicación de arriendo de "Ana" en estado "Publicada"

  Regla: Solo un visitante registrado puede pedir visita a una publicación publicada

    Escenario: Un visitante pide una visita
      Cuando "Víctor" pide una visita a la publicación de "Ana" proponiendo las franjas:
        | franja           |
        | 2026-10-07 10:00 |
        | 2026-10-08 15:00 |
      Entonces la visita queda en estado "Esperando al anfitrión"
      Y la visita tiene 2 franjas propuestas
      Y el plazo para responder vence el "2026-10-06 10:00"
      Y se notifica "visita.solicitada" a "Ana"

    Escenario: Un usuario que no es visitante no puede pedir visita
      Cuando "Pedro" pide una visita a la publicación de "Ana" para el "2026-10-07 10:00"
      Entonces la solicitud se rechaza por validación con el mensaje "Solo los visitantes pueden pedir una visita."

    Escenario: No se puede pedir visita a una publicación que no está publicada
      Dado que "Ana" pausó la publicación
      Cuando "Víctor" pide una visita a la publicación de "Ana" para el "2026-10-07 10:00"
      Entonces la solicitud se rechaza por validación con el mensaje "Solo se pueden visitar publicaciones publicadas."

  Regla: Las franjas duran una hora, entre 24 horas y 14 días de antelación, en horario de 7:00 a 19:00

    Esquema del escenario: Una franja inválida se rechaza
      Cuando "Víctor" pide una visita a la publicación de "Ana" para el "<franja>"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "<mensaje>"

      Ejemplos:
        | franja           | mensaje                                                                             |
        | 2026-10-06 08:00 | Cada franja debe empezar en al menos 24 horas.                                      |
        | 2026-10-20 10:00 | Las franjas no pueden estar a más de 14 días.                                       |
        | 2026-10-07 19:00 | La franja dura una hora y debe empezar entre las 7:00 y las 18:00 hora de Colombia. |

    Escenario: No se pueden proponer más de 3 franjas
      Cuando "Víctor" pide una visita a la publicación de "Ana" proponiendo las franjas:
        | franja           |
        | 2026-10-07 10:00 |
        | 2026-10-07 12:00 |
        | 2026-10-08 10:00 |
        | 2026-10-08 12:00 |
      Entonces la solicitud se rechaza por validación

    Escenario: Las franjas propuestas no pueden repetirse
      Cuando "Víctor" pide una visita a la publicación de "Ana" proponiendo las franjas:
        | franja           |
        | 2026-10-07 10:00 |
        | 2026-10-07 10:00 |
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Las franjas propuestas no pueden solaparse."

  Regla: Quien tiene el turno acepta una franja propuesta o propone otras

    Antecedentes:
      Dado que "Víctor" pidió una visita a la publicación de "Ana" proponiendo las franjas:
        | franja           |
        | 2026-10-07 10:00 |
        | 2026-10-08 15:00 |

    Escenario: El anfitrión acepta una de las franjas
      Cuando "Ana" agenda la visita para el "2026-10-07 10:00"
      Entonces la visita queda en estado "Agendada"
      Y la visita está agendada de "2026-10-07 10:00" a "2026-10-07 11:00"
      Y se notifica "visita.agendada" a "Víctor"

    Escenario: El anfitrión propone otras franjas y el visitante acepta
      Cuando "Ana" propone otras franjas:
        | franja           |
        | 2026-10-09 08:00 |
      Entonces la visita queda en estado "Esperando al visitante"
      Y se notifica "visita.contrapropuesta" a "Víctor"
      Cuando "Víctor" agenda la visita para el "2026-10-09 08:00"
      Entonces la visita queda en estado "Agendada"
      Y se notifica "visita.agendada" a "Ana"
      Y el historial de la visita tiene 3 entradas

    Escenario: No se puede responder fuera de turno
      Cuando "Víctor" agenda la visita para el "2026-10-07 10:00"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "No es tu turno de responder a esta visita."

    Escenario: Solo se puede aceptar una franja propuesta
      Cuando "Ana" agenda la visita para el "2026-10-09 08:00"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "La franja elegida no está entre las propuestas."

    Escenario: No se puede responder cuando el plazo ya venció
      Cuando el reloj avanza hasta las "2026-10-06 11:00"
      Y "Ana" agenda la visita para el "2026-10-07 10:00"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "El plazo para responder a esta visita ya venció."

    Escenario: Una visita agendada ya no admite más propuestas
      Dado que "Ana" agendó la visita para el "2026-10-07 10:00"
      Cuando "Víctor" propone otras franjas:
        | franja           |
        | 2026-10-09 08:00 |
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "La visita ya no está en negociación."

  Regla: Solo los participantes y moderación ven la visita

    Antecedentes:
      Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"

    Escenario: Un tercero no puede consultar la visita
      Cuando "Pedro" consulta la visita
      Entonces la solicitud se rechaza por permisos

    Escenario: Un tercero no puede responder a la visita
      Cuando "Pedro" agenda la visita para el "2026-10-07 10:00"
      Entonces la solicitud se rechaza por permisos

    Esquema del escenario: Cada usuario lista solo las visitas en las que participa
      Cuando "<usuario>" lista las visitas
      Entonces la lista contiene <visitas> visitas

      Ejemplos:
        | usuario | visitas |
        | Víctor  | 1       |
        | Ana     | 1       |
        | Pedro   | 0       |
        | Marta   | 1       |
