# Tesis

Sistema de identificación y autenticación multipasos para la gestión de documentos 
personales en diferentes áreas de servicios públicos basados en una red global de datos.

Aplicación web desarrollada con **Blazor** para la gestión de documentos personales y su consulta mediante roles y autenticación. El sistema permite cargar documentos, visualizarlos, asignar identificadores RFID/NFC y controlar el acceso según el tipo de usuario.

## Descripción general

Esta aplicación está diseñada para la digitalización de la documentación personal. Con el fin de lograr portarla mediante un accesorio con la incorporación de tecnología NFC. 

Además, incluye funcionalidades para:

- Registro e inicio de sesión de usuarios
- Asignación de RFID/NFC a usuarios
- Búsqueda de documentos usando un dispositivo serial/ESP
- Visualización y eliminación de documentos asociados a un usuario

## Tecnologías utilizadas

- [.NET 10](https://dotnet.microsoft.com/)
- [Blazor Server](https://learn.microsoft.com/aspnet/core/blazor)
- MySQL + MySqlConnector
- Acceso a puertos seriales (`System.IO.Ports`) para comunicación con ESP32

## Configuración

1. Ajusta la configuración de la base de datos en [Services/UserServices.cs](Services/UserServices.cs).

2. Si tu dispositivo serial no usa el mismo puerto, cambia la configuración en [Program.cs](Program.cs).

## Ejecución

Modo desarrollo:
```bash
 dotnet watch run
```

O bien:
```bash
 dotnet run
```


## Notas importantes

- La conexión a la base de datos y los secretos deben configurarse correctamente antes de ejecutar la aplicación.
- Si usas dispositivos RFID/NFC o seriales, verifica que el puerto configurado sea el correcto para tu equipo.
- Para entornos de producción, evita dejar credenciales sensibles hardcodeadas en el código.
