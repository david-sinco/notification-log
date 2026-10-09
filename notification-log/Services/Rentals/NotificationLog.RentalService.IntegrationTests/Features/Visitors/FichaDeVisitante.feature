# language: es
Característica: Ficha del visitante
  La ficha reúne las visitas del visitante, sus inasistencias, lo que sigue y su actividad.
  Todo sale del registro del visitante, que se mantiene al día con cada paso de sus visitas.

  Antecedentes:
    Dado que ahora son las "2026-10-05 09:00" hora de Colombia
    Y el usuario "Ana" con rol Propietario
    Y el usuario "Víctor" con rol Visitor
    Y el usuario "Valeria" con rol Visitor
    Y el usuario "Marta" con rol Moderador
    Y que "Ana" está registrada como propietaria
    Y una publicación de arriendo de "Ana" en estado "Publicada"

  Escenario: La ficha muestra la visita que pidió el visitante
    Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
    Cuando "Marta" consulta el registro de visitante de "Víctor"
    Entonces el visitante tiene 1 visita y 0 inasistencias
    Y la visita del visitante está en estado "Esperando al anfitrión"
    Y lo que sigue para el visitante es "Esperando al anfitrión"

  Escenario: Cuando el anfitrión propone otras franjas, al visitante le toca responder
    Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
    Cuando "Ana" propone otras franjas:
      | franja           |
      | 2026-10-08 15:00 |
    Y "Marta" consulta el registro de visitante de "Víctor"
    Entonces la visita del visitante está en estado "Esperando al visitante"
    Y lo que sigue para el visitante es "Esperando al visitante"

  Escenario: Una inasistencia queda en el registro del visitante
    Dada una visita de "Víctor" a la publicación de "Ana" agendada para el "2026-10-07 10:00"
    Cuando el reloj avanza hasta las "2026-10-07 11:30"
    Y "Ana" marca que el visitante no asistió
    Y "Marta" consulta el registro de visitante de "Víctor"
    Entonces el visitante tiene 1 visita y 1 inasistencia
    Y la visita del visitante está en estado "No asistió"
    Y el visitante no tiene nada pendiente

  Escenario: La actividad cuenta lo que pasó, de lo más reciente a lo más antiguo
    Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
    Y que "Ana" agendó la visita para el "2026-10-07 10:00"
    Cuando "Víctor" confirma el correo "nuevo@example.com"
    Y "Marta" consulta el registro de visitante de "Víctor"
    Entonces la actividad del visitante es:
      | acción           |
      | cambió su correo |
      | agendó la visita |
      | pidió una visita |
      | se registró      |

  Escenario: Moderación filtra visitantes con y sin visitas
    Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
    Cuando "Marta" lista los visitantes con visitas
    Entonces la lista contiene 1 visitante
    Cuando "Marta" lista los visitantes sin visitas
    Entonces la lista contiene 1 visitante

  Escenario: Moderación ordena los visitantes por más visitas
    Dado que "Víctor" pidió una visita a la publicación de "Ana" para el "2026-10-07 10:00"
    Cuando "Marta" lista los visitantes por más visitas
    Entonces el primer visitante de la lista es "Víctor"
