# language: es
Característica: Cancelación y cierre de una visita
  Cualquiera de las dos partes puede cancelar antes de que la visita empiece.
  Después de la hora agendada, el anfitrión la marca como realizada o como inasistencia.

  Antecedentes:
    Dado que ahora son las "2026-10-05 09:00" hora de Colombia
    Y el usuario "Ana" con rol Propietario
    Y el usuario "Víctor" con rol Visitor
    Y que "Ana" está registrada como propietaria
    Y una publicación de arriendo de "Ana" en estado "Publicada"
    Y que "Víctor" completó su perfil de visitante

  Regla: Una visita en negociación o agendada se puede cancelar indicando el motivo

    Escenario: El anfitrión cancela una visita en negociación
      Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
      Cuando "Ana" cancela la visita con el motivo "El inmueble ya no está disponible"
      Entonces la visita queda en estado "Cancelada"
      Y la visita fue cancelada por el anfitrión con el motivo "El inmueble ya no está disponible"
      Y se notifica "visita.cancelada" a "Víctor"

    Escenario: Cancelar exige un motivo
      Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
      Cuando "Víctor" cancela la visita con el motivo ""
      Entonces la solicitud se rechaza por validación

    Escenario: Cancelar una visita ya cancelada no tiene efecto
      Dada una visita de "Víctor" a la publicación de "Ana" cancelada por "Ana"
      Cuando "Víctor" cancela la visita con el motivo "Ya no me interesa"
      Entonces la solicitud se acepta
      Y la visita fue cancelada por el anfitrión
      Y no se envía ninguna notificación nueva

  Regla: Es cancelación tardía si el visitante cancela a menos de 12 horas de la visita

    Antecedentes:
      Dada una visita de "Víctor" a la publicación de "Ana" agendada para el "2026-10-07 10:00"

    Esquema del escenario: Cancelación de una visita agendada
      Cuando el reloj avanza hasta las "<momento>"
      Y "<quién>" cancela la visita con el motivo "No puedo asistir"
      Entonces la visita queda en estado "Cancelada"
      Y la cancelación <resultado>

      Ejemplos:
        | quién  | momento          | resultado    |
        | Víctor | 2026-10-06 21:00 | no es tardía |
        | Víctor | 2026-10-07 08:00 | es tardía    |
        | Ana    | 2026-10-07 08:00 | no es tardía |

    Escenario: No se puede cancelar una visita que ya empezó
      Cuando el reloj avanza hasta las "2026-10-07 10:30"
      Y "Víctor" cancela la visita con el motivo "No puedo asistir"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "La visita ya empezó; no se puede cancelar."

  Regla: Solo el anfitrión cierra la visita, y solo después de la hora agendada

    Antecedentes:
      Dada una visita de "Víctor" a la publicación de "Ana" agendada para el "2026-10-07 10:00"

    Escenario: El anfitrión marca la visita como realizada
      Cuando el reloj avanza hasta las "2026-10-07 11:30"
      Y "Ana" marca la visita como realizada
      Entonces la visita queda en estado "Realizada"
      Y se notifica "visita.realizada" a "Víctor"

    Escenario: El anfitrión marca que el visitante no asistió
      Cuando el reloj avanza hasta las "2026-10-07 11:30"
      Y "Ana" marca que el visitante no asistió
      Entonces la visita queda en estado "No asistió"
      Y se notifica "visita.inasistencia" a "Víctor"

    Escenario: No se puede cerrar una visita que todavía no ha empezado
      Cuando "Ana" marca la visita como realizada
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "La visita todavía no ha empezado."

    Escenario: El visitante no puede cerrar la visita
      Cuando el reloj avanza hasta las "2026-10-07 11:30"
      Y "Víctor" marca la visita como realizada
      Entonces la solicitud se rechaza por permisos

    Escenario: Una visita cerrada no se puede volver a cerrar
      Cuando el reloj avanza hasta las "2026-10-07 11:30"
      Y "Ana" marca la visita como realizada
      Y "Ana" marca que el visitante no asistió
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Solo se puede cerrar una visita agendada."
