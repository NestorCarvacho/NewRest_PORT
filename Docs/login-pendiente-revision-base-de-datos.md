# Login — integración con HOTELDB

El acceso a datos del ingreso se concentra en procedimientos almacenados del módulo `Cuenta`. El contrato de `HOTELDB` se contrastó en LocalDB y quedó implementado en `SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql`.

Los AS-IS no se copian. Se leyeron desde `C:\Users\nesto\Downloads\SGH_old-main\SGH_old-main\Docs\as-is`.

## Qué ya hace el código

1. Dos envíos del mismo formulario, como `Login.js`. El primero verifica RUT y clave y deja el formulario en solo lectura. El segundo vuelve a verificar la clave y abre la sesión. El botón dice «Entrar» en los dos pasos. La comparación MD5 o BCrypt y la flag `Md5Retirado` ocurren dentro de esa verificación, en los dos envíos.
2. El RUT se recorta y se le quita solo el primer `-`, igual que `String.replace('-','')` en el submit legado. Los puntos se conservan.
3. Estado `0` continúa. Estado `-2` muestra «Su Clave ha Caducado» en el mismo formulario. El enlace «Cambiar Contraseña» sigue aparte. Otro estado muestra «Se ha producido un error al validar el estado del usuario».
4. Si `Md5Retirado` es 0 y el hash es MD5 (32 hex, ASCII, minúsculas), la clave se verifica con esa huella y se llama a `sp_Usuario_ReemplazarHash`, que guarda BCrypt y deja la flag en 1. Si el reemplazo falla, el ingreso sigue y la fila permanece en MD5.
5. Si el hash ya es BCrypt y la flag sigue en 0, se verifica solo con BCrypt y se llama a `sp_Usuario_RetirarMd5` para dejar la flag en 1, sin reescribir la clave.
6. Si `Md5Retirado` ya es 1, no se calcula MD5. Una fila marcada y que todavía tenga un hash de 32 hex no entra.
7. El segundo «Entrar» es obligatorio tras validar RUT y clave (incluido `ideauno`). El combo «Seleccione Campamento/Perfil» aparece en ese segundo paso solo si la lista tiene filas. Si hay filas y el envío sigue en `0`, responde «Seleccione Campamento/Perfil». Con lista vacía se muestra «La sesión no tiene un campamento.» y el ingreso se rechaza. Un id que no está en la lista responde «Credenciales inválidas». La cookie no guarda la clave. La lista y la clave quedan 5 minutos: la lista en TempData y la clave solo en memoria del servidor. El segundo paso muestra una máscara, no la clave.
8. El valor del combo es el `Id` de la fila campamento-perfil, igual que `Login.js` (`value: a.id`), no el `PerfilId`.
9. El RUT `767095309` fuerza el perfil `11` antes de resolver el campamento.
10. El nombre `ideauno` entra con rol `ideauno` y sin campamento, recién en el segundo envío. No está el contrato externo que devolvía solo RUT y token.
11. Perfiles `4` y `5` usan `Sesion:MinutosOperaciones`. El resto usa `Sesion:MinutosPorDefecto`. En `appsettings.json` quedaron 480 y 60 porque el legado leía esos minutos desde `AppSettings` y ese archivo no está en el código.
12. Cambio de clave: formato de `PasswordService`, confirmación con el texto «Password debe ser la misma» y rechazo de claves de las últimas cinco entradas del historial, tanto MD5 heredado como BCrypt.
13. La clave nueva se guarda con BCrypt y el procedimiento deja `Md5Retirado = 1`. La huella MD5 de la clave nueva se envía solo cuando la fila todavía está en MD5. La aplicación verifica todo el historial reciente y el procedimiento conserva una comprobación MD5 adicional.
14. Cinco claves incorrectas del mismo RUT bloquean la cuenta 30 minutos. El aviso dice cuántos intentos quedan (`Clave incorrecta. Quedan N intentos.` y, si queda uno, `Queda 1 intento.`). En el quinto: `La cuenta quedó bloqueada por 30 minutos.` Un RUT que no existe sigue en `Credenciales inválidas`, sin contador. Una clave correcta, o el vencimiento de los 30 minutos, vuelve el contador a cero. El ingreso y el cambio de clave usan el mismo contador. Mientras está bloqueada, ni la clave correcta entra.

Mensajes de formato que devuelve el código, copiados del API y no de la pantalla React (esa pantalla decía «Contraseña dede tener…» y «letras»):

- largo menor a 6: `Clave debe ser mayor a 6` (6 caracteres sí es válido; el texto dice «mayor»);
- menos de 3 letras ASCII: `Clave debe tener al menos 3 caracteres`;
- menos de 3 dígitos: `Clave debe tener al menos 3 números`;
- dos caracteres iguales seguidos: `Clave no debe tener caracteres consecutivos`.

El tope de 10 existe en el legado como `IsLargoMayor` y no se ejecuta. Este código tampoco lo aplica.

## Cadena de conexión

En desarrollo, `SGH.Web/appsettings.Development.json` apunta a `(localdb)\MSSQLLocalDB` y `HOTELDB`. Para usar otro servidor, carga la cadena fuera del repositorio:

```powershell
dotnet user-secrets set "ConnectionStrings:Hoteldb" "Server=<servidor>;Database=HOTELDB;User Id=<usuario>;Password=<secreto>;TrustServerCertificate=True;Encrypt=False;Application Name=SGH.Web" --project SGH.Web
```

No guardes credenciales en archivos versionados. `FabricaConexionSql` requiere `ConnectionStrings:Hoteldb`.

## Procedimientos

| Procedimiento | Contrato implementado |
|---|---|---|
| `sp_Usuario_ValidarEstado` | Envuelve `USU_ValidaEstadoUsuario` y expone el código de salida como `Estado` (`0`, `-2` u otro). |
| `sp_Usuario_ObtenerCampamentosYPerfiles` | Envuelve `USU_ObtieneCampamentosYPerfilesUsuario`. Conserva `Id` como valor del combo y expone también `UsuarioId`/`UsuarioNombre` para asociar la sesión al usuario de la asignación elegida. |
| `sp_Usuario_ObtenerParaLogin` | Devuelve la fila activa con clave no vacía, con los alias que consume Dapper. En RUT duplicados usa `FechaCreacion DESC, Id DESC`; la identidad final se resuelve con el perfil seleccionado. |
| `sp_Usuario_ActualizarPassword` | Guarda BCrypt, agrega la fila al historial, retira MD5 y limpia bloqueo/intentos. Devuelve `0`, `-3` o un código distinto de éxito. |
| `sp_Usuario_CambiarPasswordPorRut` | Cambio administrativo por RUT. Recibe únicamente un hash BCrypt generado por la aplicación, actualiza la cuenta activa más reciente, agrega historial, retira MD5 y limpia el bloqueo. |
| `sp_Usuario_ReemplazarHash` | Migra las filas MD5 del RUT a BCrypt sin aplicar la regla de reutilización. |
| `sp_Usuario_RetirarMd5` | Marca como retirado MD5 cuando la fila ya contiene BCrypt. |
| `sp_Usuario_ObtenerHistorialContrasenas` | Lee el número de entradas configurado en `Listas.usrpasshist`, ordenado por fecha e Id. Application compara MD5 heredado y BCrypt. |
| `sp_Usuario_RegistrarIntentoFallido` | Reinicia un bloqueo vencido, suma el intento y bloquea al quinto intento durante 30 minutos. |
| `sp_Usuario_ReiniciarIntentos` | Reinicia el contador y elimina el bloqueo al validar la clave. |

## Contrato de HOTELDB contrastado

- `dbo.Usuarios` contiene `Id`, `Rut`, `Nombre`, `Apellidos`, `PerfilId`, `Password` (inicialmente `nvarchar(50)`), `IsActive` y `EmpresaMandanteId`. El script amplía `Password` a `nvarchar(100)`.
- `dbo.HistoricoPassword.Password` también era `nvarchar(50)` y se amplía a `nvarchar(100)` para admitir hashes BCrypt de 60 caracteres.
- `dbo.HistoricoPassword` contiene `Rut`, `Password`, `FechaCreacion` y `UsuarioCreacion`. `fnc_ValidaPassword` toma la cantidad de entradas de `Listas.usrpasshist` (actualmente 5). Los valores existentes revisados son huellas MD5.
- `USU_ValidaEstadoUsuario` devuelve `0` o `-2` según la vigencia de la última clave. El procedimiento legado de autenticación exige `IsActive = 1`.
- El procedimiento legado de perfiles devuelve el `UsuarioId` asociado a cada fila. La sesión usa ese ID y su nombre cuando el usuario escoge una asignación, evitando asociar RUT duplicados a una cuenta arbitraria.
- El script agrega idempotentemente `Md5Retirado`, `IntentosFallidos` y `BloqueadaHasta`, y crea/actualiza los procedimientos nuevos.

## Aplicación

`SGH.Infrastructure/Cuenta/Persistence/001_sp_login.sql` quedó aplicado en la base LocalDB `HOTELDB`. Es idempotente para poder repetir la instalación. En una base productiva, revisa y ejecuta el script mediante el proceso de despliegue aprobado.

Pruebas funcionales recomendadas: clave MD5 vigente y vencida, varios perfiles, RUT `767095309`, cambio de clave, reutilización de claves MD5 y BCrypt, cuatro intentos fallidos y bloqueo al quinto.

El cambio administrativo recibe un hash BCrypt, nunca una contraseña en claro:

```sql
DECLARE @Ret int;
EXEC dbo.sp_Usuario_CambiarPasswordPorRut
    @Rut = @Rut,
    @PasswordHash = @HashBcrypt,
    @Ret = @Ret OUTPUT;
SELECT @Ret AS Retorno;
```

## Qué no quedó en este corte

Sigue vigente en el AS-IS y no tiene pantalla ni procedimiento nuevo:

- ESS: `POST /api/usuarios/authenticate-ess` con RUT, token, campamento y correo. Resultado distinto de cero es error. El legado emite JWT con `Sistema = ESS`. Procedimientos: `USU_ObtieneUsuarioAuthEss`.
- Respuesta de integración de `ideauno`: el API devolvía solo RUT y token. Aquí ese usuario abre cookie con rol `ideauno`.
- Política de aceptación durante el preingreso. Se muestra solo si `SolicitaPolitica` es verdadero. La versión `app_version_number` se normaliza a `0.0`. La aceptación guarda IP y user-agent. Procedimientos: `POL_ObtenerPoliticaFromRut`, `POL_ObtenerPoliticaById` y el de guardado. Configuración `Politica:ShowTerminosyCondiciones`, por defecto verdadera.
- `GET /api/usuarios/mensajeAutorizacion`: título y cuerpo salen de configuración, no de una tabla.
- Alta de usuarios. Ahí el perfil `2` puede crearse sin clave, y un RUT repetido se rechaza aunque el usuario esté inactivo. El borrado del API no persiste.
- Fila de auditoría `accion = autenticacion` que el API escribía después de autenticar. No hay procedimiento de log en este corte: un borrador que haga `RAISERROR` cortaría el ingreso.
