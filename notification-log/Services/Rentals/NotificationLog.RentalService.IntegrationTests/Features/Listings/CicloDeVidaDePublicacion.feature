# language: es
Característica: Ciclo de vida de una publicación
  El propietario crea la publicación en borrador, la completa y la envía a revisión.
  Una vez publicada puede pausarla, renovarla, cerrarla o retirarla.

  Antecedentes:
    Dado que ahora son las "2026-10-05 09:00" hora de Colombia
    Y el usuario "Ana" con rol Propietario
    Y el usuario "Pedro" con rol Propietario
    Y el usuario "Marta" con rol Moderador
    Y que "Ana" está registrada como propietaria
    Y que "Pedro" está registrado como propietario

  Regla: Una publicación nace en borrador a nombre de un propietario registrado

    Escenario: Un propietario crea una publicación a su nombre
      Cuando "Ana" crea una publicación de arriendo a su nombre
      Entonces la publicación queda en estado "Borrador"
      Y la publicación pertenece a "Ana"

    Escenario: Un propietario no puede crear publicaciones a nombre de otro
      Cuando "Ana" crea una publicación de arriendo a nombre de "Pedro"
      Entonces la solicitud se rechaza por permisos

    Escenario: Un moderador crea una publicación a nombre de un propietario
      Cuando "Marta" crea una publicación de venta a nombre de "Ana"
      Entonces la publicación queda en estado "Borrador"
      Y la publicación pertenece a "Ana"

    Escenario: No se puede crear una publicación para un propietario que no existe
      Cuando "Marta" crea una publicación de arriendo a nombre de un propietario inexistente
      Entonces la solicitud se rechaza por validación con el mensaje "El propietario no está registrado."

  Regla: Para enviarla a revisión necesita datos, precio y al menos 5 fotos

    Escenario: Una publicación completa se envía a revisión
      Dada una publicación de arriendo de "Ana" recién creada
      Cuando "Ana" actualiza los datos del inmueble con:
        | tipo      | área | habitaciones | baños | parqueaderos | estrato | piso | ascensor | administración | ciudad | barrio    | dirección        |
        | Apartment | 68   | 2            | 2     | 1            | 4       | 5    | sí       | 320000         | Bogotá | Chapinero | Calle 60 # 9-45  |
      Y "Ana" fija el precio en 1800000
      Y "Ana" sube 5 fotos
      Y "Ana" envía la publicación a revisión
      Entonces la publicación queda en estado "En revisión"
      Y la publicación tiene 5 fotos
      Y la publicación tiene precio 1800000

    Esquema del escenario: Una publicación incompleta no se envía a revisión
      Dada una publicación de arriendo de "Ana" en estado "Borrador" <carencia>
      Cuando "Ana" envía la publicación a revisión
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "<mensaje>"

      Ejemplos:
        | carencia               | mensaje                                                       |
        | sin datos del inmueble | Faltan los datos del inmueble, la ubicación o la descripción. |
        | sin precio             | Falta el precio de la publicación.                            |
        | con solo 4 fotos       | La publicación necesita al menos 5 fotos.                     |

    Escenario: El canon de arriendo tiene un mínimo
      Dada una publicación de arriendo de "Ana" en estado "Borrador"
      Cuando "Ana" fija el precio en 299999
      Entonces la solicitud se rechaza por regla de negocio

    Escenario: Una descripción demasiado corta no pasa la validación
      Dada una publicación de arriendo de "Ana" en estado "Borrador"
      Cuando "Ana" actualiza los datos del inmueble con una descripción de 50 caracteres
      Entonces la solicitud se rechaza por validación

  Regla: Solo el propietario de la publicación, un administrador o un moderador pueden gestionarla

    Escenario: Otro propietario no puede editar la publicación
      Dada una publicación de arriendo de "Ana" en estado "Borrador"
      Cuando "Pedro" fija el precio en 1800000
      Entonces la solicitud se rechaza por permisos

    Escenario: Un moderador puede gestionar la publicación de un propietario
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Marta" pausa la publicación
      Entonces la publicación queda en estado "Pausada"

  Regla: Editar una publicación visible la devuelve a revisión

    Escenario: Cambiar los datos de una publicación publicada la devuelve a revisión
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" actualiza los datos del inmueble cambiando el barrio a "Usaquén"
      Entonces la publicación queda en estado "En revisión"
      Y la publicación no aparece en el catálogo

    Escenario: Añadir una foto a una publicación publicada la devuelve a revisión
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" sube 1 foto
      Entonces la publicación queda en estado "En revisión"

    Escenario: Cambiar el precio no la devuelve a revisión
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" fija el precio en 1950000
      Entonces la publicación queda en estado "Publicada"
      Y la publicación tiene precio 1950000

    Escenario: El precio de una publicación publicada solo cambia una vez cada 24 horas
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Y que "Ana" fijó el precio en 1950000
      Cuando pasan 23 horas
      Y "Ana" fija el precio en 2000000
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "El precio solo se puede cambiar una vez cada 24 horas."
      Cuando pasan 2 horas
      Y "Ana" fija el precio en 2000000
      Entonces la publicación tiene precio 2000000

  Regla: Las fotos se pueden quitar y reordenar

    Escenario: Reordenar las fotos
      Dada una publicación de arriendo de "Ana" en estado "Borrador" con 5 fotos
      Cuando "Ana" reordena las fotos poniendo la última en primer lugar
      Entonces la primera foto de la publicación es la que antes era la última

    Escenario: Quitar una foto
      Dada una publicación de arriendo de "Ana" en estado "Borrador" con 5 fotos
      Cuando "Ana" quita la primera foto
      Entonces la publicación tiene 4 fotos

  Regla: Una publicación publicada se puede pausar y reanudar

    Escenario: Pausar y reanudar
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" pausa la publicación
      Entonces la publicación queda en estado "Pausada"
      Y la publicación no aparece en el catálogo
      Cuando "Ana" reanuda la publicación
      Entonces la publicación queda en estado "Publicada"
      Y la publicación aparece en el catálogo

    Escenario: No se puede pausar un borrador
      Dada una publicación de arriendo de "Ana" en estado "Borrador"
      Cuando "Ana" pausa la publicación
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Solo se puede pausar una publicación en estado publicado."

  Regla: La vigencia es de un mes y se renueva cuando quedan 7 días o menos

    Escenario: Renovar dentro de la ventana extiende la vigencia un mes
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando pasan 25 días
      Y "Ana" renueva la publicación
      Entonces la publicación vence el "2026-11-30"

    Escenario: No se puede renovar antes de la ventana
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando pasan 10 días
      Y "Ana" renueva la publicación
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Solo se puede renovar una publicación vencida o a la que le queden 7 días o menos."

  Regla: Una publicación visible se cierra cuando se arrienda o se vende

    Escenario: Cerrar una publicación
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" cierra la publicación con valor final 1750000 firmada el "2026-10-04"
      Entonces la publicación queda en estado "Cerrada"
      Y la publicación no aparece en el catálogo

    Escenario: La fecha de firma no puede estar en el futuro
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" cierra la publicación con valor final 1750000 firmada el "2026-10-20"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "La fecha de firma no puede estar en el futuro."

    Escenario: Una publicación cerrada ya no se puede retirar
      Dada una publicación de arriendo de "Ana" en estado "Cerrada"
      Cuando "Ana" retira la publicación con el motivo "Ya no está disponible"
      Entonces la solicitud se rechaza por regla de negocio con el mensaje "Una publicación cerrada no se puede retirar."

  Regla: El propietario puede retirar su publicación indicando el motivo

    Escenario: Retirar una publicación
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" retira la publicación con el motivo "Decidí no arrendar"
      Entonces la publicación queda en estado "Retirada"
      Y la publicación no aparece en el catálogo

    Escenario: Retirar exige un motivo
      Dada una publicación de arriendo de "Ana" en estado "Publicada"
      Cuando "Ana" retira la publicación con el motivo ""
      Entonces la solicitud se rechaza por validación

  Escenario: Una publicación inexistente no se encuentra
    Cuando "Ana" consulta una publicación inexistente
    Entonces la publicación no se encuentra
