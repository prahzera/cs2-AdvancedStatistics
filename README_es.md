# Advanced Statistics Plugin para CS2

[![English](https://img.shields.io/badge/README-English-blue)](README.md)
[![Español](https://img.shields.io/badge/README-Español-red)](README_es.md)

Plugin avanzado de estadísticas para Counter-Strike 2 que rastrea y almacena datos detallados de juego en una base de datos MySQL.

## Características

- Rastreo detallado de kills, muertes y asistencias por arma específica
- Almacenamiento en base de datos MySQL
- Configuración personalizable
- Opción para incluir o excluir eventos con bots
- Estadísticas en tiempo real

## Requisitos

- Counter-Strike Sharp
- Servidor MySQL
- .NET 7.0

## Instalación

1. Compilar el plugin
2. Copiar los archivos a la carpeta de plugins de Counter-Strike Sharp
3. Configurar la base de datos en el archivo de configuración
4. Reiniciar el servidor

## Configuración

El archivo de configuración se encuentra en `configs/plugins/AdvancedStatistics/AdvancedStatistics.json`:

```json
{
  "DatabaseHost": "localhost",
  "DatabasePort": 3306,
  "DatabaseName": "cs2_stats",
  "DatabaseUser": "root",
  "DatabasePassword": "",
  "TrackBotEvents": false,
  "UpdateInterval": 30
}
```

## Estructura de la Base de Datos

El plugin crea automáticamente la tabla `player_stats` con columnas para:

- Estadísticas generales (kills, deaths, assists, headshots, etc.)
- Kills por arma específica (ak47_kills, m4a4_kills, etc.)
- Muertes por arma específica (ak47_deaths, m4a4_deaths, etc.)
- Asistencias por arma específica (ak47_assists, m4a4_assists, etc.)

## Uso

Una vez instalado y configurado, el plugin comenzará a rastrear automáticamente los eventos del juego y almacenarlos en la base de datos.

## Desarrollo

Para compilar el plugin:

```bash
dotnet build
```

## Contribuir

1. Fork del repositorio
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

## Licencia

Este proyecto está licenciado bajo la Licencia MIT.