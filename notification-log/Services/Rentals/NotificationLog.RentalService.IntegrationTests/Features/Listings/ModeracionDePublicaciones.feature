# language: es
Característica: Moderación de publicaciones
  Un moderador o administrador revisa las publicaciones antes de que salgan al catálogo
  y puede suspenderlas, restablecerlas o retirarlas después.

  Antecedentes:
    Dado que ahora son las "2026-10-05 09:00" hora de Colombia
    Y el usuario "Ana" con rol Propietario
    Y el usuario "Marta" con rol Moderador
    Y el usuario "Adriana" con rol Administrador
    Y que "Ana" está registrada como propietaria

  Regla: Solo moderadores y administradores pueden moderar

    Esquema del escenario: Moderadores y administradores aprueban publicaciones
      Dada una publicación de arriendo de "Ana" en estado "En revisión"
      Cuando "<usuario>" aprueba la publicación
      Entonces la publicación queda en estado "Publicada"

      Ejemplos:
        | usuario |
        | Marta   |
        | Adriana |

    Escenario: Un propietario no puede aprobar su propia publicación
      Dada una publicación de arriendo de "Ana" en estado "En revisión"
      Cuando "Ana" aprueba la publicación
      Entonces la solicitud se rechaza por permisos
      Y la publicación queda en estado "En revisión"

  Regla: Aprobar publica la publicación con un mes de vigencia

    Escenario: Una publicación aprobada aparece en el catálogo
      Dada una publicación de arriendo de "Ana" en estado "En revisión"
      Cuando "Marta" aprueba la publicación
      Entonces la publicación queda en estado "Publicada"
      Y la publicación vence el "2026-11-05"
      Y la publicación aparece en el catálogo

    Escenario: Solo se aprueba una publicación en revisión
      Dada una publicación de arriendo de "Ana" en estado "Borrador"
      Cuando "Marta" aprueba la publicación
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Solo se puede aprobar una publicación en revisión."

  Regla: Rechazar devuelve la publicación a borrador con sus motivos

    Escenario: Rechazar una publicación en revisión
      Dada una publicación de arriendo de "Ana" en estado "En revisión"
      Cuando "Marta" rechaza la publicación por los motivos:
        | motivo           |
        | LowQualityPhotos |
        | InvalidAddress   |
      Entonces la publicación queda en estado "Borrador"
      Y la publicación no aparece en el catálogo

    Escenario: Una publicación rechazada se puede corregir y volver a enviar
      Dada una publicación de arriendo de "Ana" rechazada por "LowQualityPhotos"
      Cuando "Ana" sube 1 foto
      Y "Ana" envía la publicación a revisión
      Entonces la publicación queda en estado "En revisión"

    Escenario: Rechazar exige al menos un motivo
      Dada una publicación de arriendo de "Ana" en estado "En revisión"
      Cuando "Marta" rechaza la publicación sin motivos
      Entonces la solicitud se rechaza por validación

  Regla: Suspender saca la publicación del catálogo hasta que se restablece

    Escenario: Suspender y restablecer una publicación
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Marta" suspende la publicación con el motivo "Denuncia por datos falsos"
      Entonces la publicación queda en estado "Suspendida"
      Y la publicación no aparece en el catálogo
      Cuando "Marta" restablece la publicación
      Entonces la publicación queda en estado "Publicada"
      Y la publicación aparece en el catálogo

    Escenario: Una publicación suspendida no se puede editar
      Dada una publicación de arriendo de "Ana" en estado "Suspendida"
      Cuando "Ana" sube 1 foto
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Solo se puede editar una publicación en borrador, en revisión, publicada o pausada."

    Escenario: Restablecer una publicación cuya vigencia venció durante la suspensión la deja vencida
      Dada una publicación de arriendo de "Ana" en estado "Suspendida"
      Cuando pasan 40 días
      Y "Marta" restablece la publicación
      Entonces la publicación queda en estado "Vencida"
      Y la publicación no aparece en el catálogo

    Escenario: Solo se suspende una publicación visible
      Dada una publicación de arriendo de "Ana" en estado "Borrador"
      Cuando "Marta" suspende la publicación con el motivo "Denuncia por datos falsos"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Solo se puede suspender una publicación publicada o pausada."

    Escenario: Solo se restablece una publicación suspendida
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Marta" restablece la publicación
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "La publicación no está suspendida."

  Regla: Moderación puede retirar una publicación

    Escenario: Un moderador retira una publicación
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Marta" retira la publicación por moderación con el motivo "Contenido prohibido reiterado"
      Entonces la publicación queda en estado "Retirada"
      Y la publicación no aparece en el catálogo
