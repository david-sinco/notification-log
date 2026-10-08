# language: es
Característica: Registro de visitantes
  Todo usuario con rol Visitor es un visitante, con una copia de su nombre, correo y teléfono.
  Los datos del visitante siguen a los del usuario y no se editan desde Rentals.

  Antecedentes:
    Dado el usuario "Víctor" con rol Visitor
    Y el usuario "Valeria" con rol Visitor
    Y el usuario "Ana" con rol Propietario
    Y el usuario "Marta" con rol Moderador

  Escenario: Un usuario con rol Visitor es visitante
    Cuando "Víctor" consulta su registro de visitante
    Entonces "Víctor" consulta su registro de visitante y ve:
      | nombre | correo             | teléfono      |
      | Víctor | víctor@example.com | +573109876543 |

  Escenario: Un usuario con otro rol no es visitante
    Cuando "Ana" consulta su registro de visitante
    Entonces el registro de visitante no se encuentra

  Escenario: Solo los usuarios con rol Visitor aparecen como visitantes
    Cuando "Marta" lista los visitantes
    Entonces la lista contiene 2 visitantes

  Escenario: El visitante toma el correo que el usuario confirma
    Cuando "Víctor" confirma el correo "nuevo@example.com"
    Entonces "Víctor" consulta su registro de visitante y ve:
      | nombre | correo            | teléfono      |
      | Víctor | nuevo@example.com | +573109876543 |

  Escenario: Confirmar el mismo correo no cambia al visitante
    Cuando "Víctor" confirma el correo "víctor@example.com"
    Entonces "Víctor" consulta su registro de visitante y ve:
      | nombre | correo             | teléfono      |
      | Víctor | víctor@example.com | +573109876543 |

  Escenario: Un visitante no puede consultar el registro de otro
    Cuando "Valeria" consulta el registro de visitante de "Víctor"
    Entonces la solicitud se rechaza por permisos

  Escenario: Moderación puede consultar y listar visitantes
    Cuando "Marta" consulta el registro de visitante de "Víctor"
    Entonces la solicitud se acepta

  Escenario: Un visitante no puede listar los visitantes
    Cuando "Víctor" lista los visitantes
    Entonces la solicitud se rechaza por permisos
