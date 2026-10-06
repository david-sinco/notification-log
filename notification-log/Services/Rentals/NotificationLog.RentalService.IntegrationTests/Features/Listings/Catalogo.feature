# language: es
Característica: Catálogo público
  Cualquier persona, sin iniciar sesión, puede ver las publicaciones publicadas.
  Las que están en cualquier otro estado no existen para el catálogo.

  Antecedentes:
    Dado que ahora son las "2026-10-05 09:00" hora de Colombia
    Y el usuario "Ana" con rol Propietario
    Y que "Ana" está registrada como propietaria

  Escenario: El catálogo solo lista publicaciones publicadas
    Dadas las siguientes publicaciones de "Ana":
      | operación | barrio      | estado      |
      | arriendo  | Chapinero   | Publicada   |
      | venta     | Usaquén     | Publicada   |
      | arriendo  | Suba        | Borrador    |
      | arriendo  | Teusaquillo | En revisión |
      | venta     | Cedritos    | Pausada     |
    Cuando un visitante anónimo consulta el catálogo
    Entonces el catálogo muestra 2 publicaciones
    Y el catálogo muestra las publicaciones de los barrios:
      | barrio    |
      | Chapinero |
      | Usaquén   |

  Escenario: El catálogo se filtra por operación
    Dadas las siguientes publicaciones de "Ana":
      | operación | barrio    | estado    |
      | arriendo  | Chapinero | Publicada |
      | venta     | Usaquén   | Publicada |
    Cuando un visitante anónimo consulta el catálogo filtrando por operación "Rent"
    Entonces el catálogo muestra 1 publicación
    Y el catálogo muestra las publicaciones de los barrios:
      | barrio    |
      | Chapinero |

  Escenario: El catálogo se puede buscar por texto
    Dadas las siguientes publicaciones de "Ana":
      | operación | barrio    | estado    |
      | arriendo  | Chapinero | Publicada |
      | venta     | Usaquén   | Publicada |
    Cuando un visitante anónimo busca "Usaquén" en el catálogo
    Entonces el catálogo muestra 1 publicación

  Escenario: El catálogo se pagina
    Dadas 12 publicaciones de arriendo de "Ana" en estado "Publicada"
    Cuando un visitante anónimo consulta la página 2 del catálogo con 10 publicaciones por página
    Entonces el catálogo muestra 2 publicaciones
    Y el total de publicaciones del catálogo es 12

  Escenario: Un visitante anónimo ve el detalle de una publicación publicada
    Dada una publicación de arriendo de "Ana" en estado "Publicada"
    Cuando un visitante anónimo consulta la publicación en el catálogo
    Entonces ve la publicación con sus 5 fotos y su precio

  Esquema del escenario: Una publicación no publicada no existe en el catálogo
    Dada una publicación de arriendo de "Ana" en estado "<estado>"
    Cuando un visitante anónimo consulta la publicación en el catálogo
    Entonces la publicación no se encuentra

    Ejemplos:
      | estado      |
      | Borrador    |
      | En revisión |
      | Pausada     |
      | Suspendida  |
      | Retirada    |
      | Cerrada     |

  Escenario: El resto de la API exige iniciar sesión
    Cuando un visitante anónimo lista las publicaciones por la API privada
    Entonces la solicitud se rechaza por falta de autenticación
