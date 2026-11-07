# Login — pendiente de contrastar con la base

El código del ingreso ya está en este repo y no consulta tablas desde C#. Toda la diferencia física queda en los procedimientos de `SGH.Infrastructure/Cuenta/Persistence/001_sp_login_borrador.sql` y, si el nombre de una columna del resultado no se puede aliasar, en las clases privadas de `ConsultaAutenticacion`.

Los AS-IS no se copian. Se leyeron desde `C:\Users\nesto\Downloads\SGH_old-main\SGH_old-main\Docs\as-is`.

## Qué ya hace el código

1. Dos envíos del mismo formulario, como `Login.js`. El primero verifica RUT y clave y deja el formulario en solo lectura. El segundo vuelve a verificar la clave y abre la sesión. El botón dice «Entrar» en los dos pasos. La comparación MD5 o BCrypt y la flag `Md5Retirado` ocurren dentro de esa verificación, en los dos envíos.
2. El RUT se recorta y se le quita solo el primer `-`, igual que `String.replace('-','')` en el submit legado. Los puntos se conservan.
3. Estado `0` continúa. Estado `-2` muestra «Su Clave ha Caducado» en el mismo formulario. El enlace «Cambiar Contraseña» sigue aparte. Otro estado muestra «Se ha producido un error al validar el estado del usuario».
4. Si `Md5Retirado` es 0 y el hash es MD5 (32 hex, ASCII, minúsculas), la clave se verifica con esa huella y se llama a `sp_Usuario_ReemplazarHash`, que guarda BCrypt y deja la flag en 1. Si el reemplazo falla, el ingreso sigue y la fila permanece en MD5.
5. Si el hash ya es BCrypt y la flag sigue en 0, se verifica solo con BCrypt y se llama a `sp_Usuario_RetirarMd5` para dejar la flag en 1, sin reescribir la clave.
6. Si `Md5Retirado` ya es 1, no se calcula MD5. Una fila marcada y que todavía tenga un hash de 32 hex no entra.
7. El segundo «Entrar» es obligatorio aunque haya cero o un campamento/perfil, y también para `ideauno`. El combo «Seleccione Campamento/Perfil» aparece solo si la lista tiene filas y parte en `0`. Con `0` o lista vacía se usa el `PerfilId` de la ficha. Un id que no está en la lista responde «Credenciales inválidas». La cookie no guarda la clave. La lista queda 5 minutos en TempData; la clave se reenvía en el segundo post.
8. El valor del combo es el `Id` de la fila campamento-perfil, igual que `Login.js` (`value: a.id`), no el `PerfilId`.
9. El RUT `767095309` fuerza el perfil `11` antes de resolver el campamento.
10. El nombre `ideauno` entra con rol `ideauno` y sin campamento, recién en el segundo envío. No está el contrato externo que devolvía solo RUT y token.
11. Perfiles `4` y `5` usan `Sesion:MinutosOperaciones`. El resto usa `Sesion:MinutosPorDefecto`. En `appsettings.json` quedaron 480 y 60 porque el legado leía esos minutos desde `AppSettings` y ese archivo no está en el código.
12. Cambio de clave: formato de `PasswordService`, confirmación con el texto «Password debe ser la misma», rechazo de la clave vigente y del código `-3`.
13. La clave nueva se guarda con BCrypt y el procedimiento deja `Md5Retirado = 1`. La huella MD5 de la clave nueva se envía solo cuando la fila todavía está en MD5, para comparar el histórico viejo. Con la flag en 1, `@HuellaMd5` va en NULL.

Mensajes de formato que devuelve el código, copiados del API y no de la pantalla React (esa pantalla decía «Contraseña dede tener…» y «letras»):

- largo menor a 6: `Clave debe ser mayor a 6` (6 caracteres sí es válido; el texto dice «mayor»);
- menos de 3 letras ASCII: `Clave debe tener al menos 3 caracteres`;
- menos de 3 dígitos: `Clave debe tener al menos 3 números`;
- dos caracteres iguales seguidos: `Clave no debe tener caracteres consecutivos`.

El tope de 10 existe en el legado como `IsLargoMayor` y no se ejecuta. Este código tampoco lo aplica.

## Cadena de conexión

Antes de probar contra el servidor:

```powershell
dotnet user-secrets set "ConnectionStrings:Hoteldb" "Server=<servidor>;Database=HOTELDB;User Id=<usuario>;Password=<secreto>;TrustServerCertificate=True;Encrypt=False;Application Name=SGH.Web" --project SGH.Web
```

Sin esa clave, el ingreso responde que falta `ConnectionStrings:Hoteldb`. No hay secreto en el repositorio.

## Procedimientos

| Procedimiento nuevo | Qué debe devolver o hacer | Legado a abrir |
|---|---|---|
| `sp_Usuario_ValidarEstado` | Una fila con columna `Estado`. El borrador ya llama a `USU_ValidaEstadoUsuario @rut, @ret OUTPUT`. | Confirmar que `@ret` es `0`, `-2` u otro entero. |
| `sp_Usuario_ObtenerCampamentosYPerfiles` | El borrador ejecuta `USU_ObtieneCampamentosYPerfilesUsuario @UsuarioRut`. | Confirmar columnas `Id`, `PerfilId`, `PerfilNombre`, `CampamentoId`, `CampamentoNombre`, `EmpresaMandanteId`, `EmpresaMandanteNombre`, `CampamentoPerfil`. `Id` es el valor del combo. |
| `sp_Usuario_ObtenerParaLogin` | Hoy hace `RAISERROR` y no lee tablas. Al habilitarlo: una fila con alias `Id`, `Rut`, `Nombre`, `Apellidos`, `HashContrasena`, `PerfilId`, `EmpresaMandanteId`, `Md5Retirado`. | `USU_ObtieneUsuarioByRutAndPass` compara la clave dentro de SQL y no sirve para traer el hash. Hay que leer la columna real. |
| `sp_Usuario_ActualizarPassword` | Hoy hace `RAISERROR`. Parámetros `@Rut`, `@PasswordHash` (BCrypt), `@HuellaMd5` (NULL si MD5 ya está retirado), `@Ret OUTPUT`. Deja `Md5Retirado = 1`. Códigos: `0` bien, `-3` clave ya usada, otro valor se muestra como credencial inválida. | `USU_CambiaPasswordUsuario` y `USU_insertaHistoricoPassword`. |
| `sp_Usuario_ReemplazarHash` | Hoy hace `RAISERROR`. Parámetros `@Rut`, `@PasswordHash`, `@Ret OUTPUT`. Guarda BCrypt de la misma clave y deja `Md5Retirado = 1`, sin control de reutilización. | No usar el cambio de clave legado para este reemplazo. |
| `sp_Usuario_RetirarMd5` | Hoy hace `RAISERROR`. Parámetros `@Rut`, `@Ret OUTPUT`. Solo pone `Md5Retirado = 1` cuando el hash ya es BCrypt. | La columna no existe en el legado. |

## Hipótesis que hay que confirmar el lunes

1. La clave está en una columna recuperable por RUT. En el script comentado se asumió `dbo.Usuarios.Password`. El nombre de tabla y de columna puede ser otro. El alias hacia `HashContrasena` se mantiene.
2. Esa columna mide 32 caracteres, el largo del MD5. BCrypt ocupa 60. Hay que ampliarla a `varchar(200)` antes de guardar el primer hash nuevo. Si no cabe, el reemplazo falla y el usuario sigue entrando con MD5.
3. Hay que agregar `Md5Retirado bit NOT NULL` con default `0` en la tabla de usuarios. El nombre físico de la tabla sigue pendiente. `0` permite el puente MD5. `1` apaga MD5 para esa fila. Los tres procedimientos de escritura la dejan en `1`.
4. El `-3` sale de un histórico. `USU_insertaHistoricoPassword @rut, @usuario` se usa en el alta y en la edición de usuario, no en `CambiaPassword`. Hay que ver si el histórico guarda MD5, la clave en claro o las dos cosas. `@HuellaMd5` existe para comparar esas filas viejas y solo se envía mientras la fila sigue en MD5. `USU_CambiaPasswordUsuario` no debe volver a hashear: si lo hace, el procedimiento nuevo escribe la columna directo.
5. `USU_ObtieneUsuarioAuth` recibe `@LoginInterno`. La web manda `0` y este código también. `USU_ObtieneUsuarioByRutAndPass` no recibe ese parámetro. Si un valor distinto de `0` cambia el usuario encontrado, hay que aplicarlo dentro de `sp_Usuario_ObtenerParaLogin`. El parámetro ya llega al procedimiento.
6. Los minutos reales de `defaultSessionTime` y `operationsSessionTime` en el `AppSettings` del API. Ajustar `Sesion` en configuración, sin tocar casos de uso.
7. El estado del usuario puede depender de vigencia, bloqueo o clave vencida por fecha, todo dentro de `USU_ValidaEstadoUsuario`. No reimplementar esa regla en C# si el envoltorio ya devuelve el entero.
8. Usuarios con más de una fila para el mismo RUT. El borrador comentado usa `TOP (1)` sin `ORDER BY`. Definir cuál fila es la de ingreso.

## Qué no quedó en este corte

Sigue vigente en el AS-IS y no tiene pantalla ni procedimiento nuevo:

- ESS: `POST /api/usuarios/authenticate-ess` con RUT, token, campamento y correo. Resultado distinto de cero es error. El legado emite JWT con `Sistema = ESS`. Procedimientos: `USU_ObtieneUsuarioAuthEss`.
- Respuesta de integración de `ideauno`: el API devolvía solo RUT y token. Aquí ese usuario abre cookie con rol `ideauno`.
- Política de aceptación durante el preingreso. Se muestra solo si `SolicitaPolitica` es verdadero. La versión `app_version_number` se normaliza a `0.0`. La aceptación guarda IP y user-agent. Procedimientos: `POL_ObtenerPoliticaFromRut`, `POL_ObtenerPoliticaById` y el de guardado. Configuración `Politica:ShowTerminosyCondiciones`, por defecto verdadera.
- `GET /api/usuarios/mensajeAutorizacion`: título y cuerpo salen de configuración, no de una tabla.
- Alta de usuarios. Ahí el perfil `2` puede crearse sin clave, y un RUT repetido se rechaza aunque el usuario esté inactivo. El borrado del API no persiste.
- Fila de auditoría `accion = autenticacion` que el API escribía después de autenticar. No hay procedimiento de log en este corte: un borrador que haga `RAISERROR` cortaría el ingreso.

## Orden sugerido el lunes

1. Abrir los cinco procedimientos `USU_*` y `POL_*` citados arriba y anotar tabla, columna de clave, largo y tabla de histórico.
2. Completar el `SELECT` de `sp_Usuario_ObtenerParaLogin` con los alias de este documento.
3. Definir el cuerpo de `sp_Usuario_ActualizarPassword` y `sp_Usuario_ReemplazarHash`.
4. Ampliar la columna si mide menos de 60.
5. Ejecutar los cinco `CREATE OR ALTER` en un ambiente de prueba.
6. Cargar la cadena con user-secrets y probar: clave MD5 vigente, clave caducada, dos perfiles, RUT `767095309`, cambio de clave y clave repetida.
7. Si una columna viene con otro nombre y no se puede aliasar en SQL, el único cambio de C# es la clase fila al final de `ConsultaAutenticacion`.
