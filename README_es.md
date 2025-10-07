# Plugin de Estadísticas Avanzadas

Un plugin de Counter-Strike 2 que registra estadísticas detalladas de los jugadores.

## Características

- Seguimiento de kills, muertes y asistencias
- Estadísticas por arma (AK-47, M4A4, M4A1-S, AWP, etc.)
- Seguimiento de daño hecho y recibido
- Estadísticas de headshots por arma
- Seguimiento detallado por partes del cuerpo (pecho, estómago, piernas)
- Soporte para eventos durante el calentamiento (configurable)
- Soporte para estadísticas de bots (configurable)

## Configuración

El plugin se puede configurar mediante el archivo `configs/plugins/AdvancedStatistics/AdvancedStatistics.json`:

```json
{
  "DatabaseHost": "localhost",
  "DatabasePort": 3306,
  "DatabaseName": "cs2_stats",
  "DatabaseUser": "user",
  "DatabasePassword": "password",
  "TrackWarmupEvents": false,
  "TrackBotEvents": false
}
```

## Comandos

- `css_stats` - Muestra las estadísticas del jugador

## Base de Datos

El plugin crea automáticamente la tabla `player_stats` con las siguientes columnas:

- Estadísticas generales (kills, deaths, assists, headshots, daño, etc.)
- Estadísticas específicas por arma (kills, deaths, assists, headshots)
- Estadísticas por tipo de impacto (headshots, chestshots, stomachshots, legshots)

## Instalación

1. Compilar el plugin
2. Copiar los archivos a la carpeta de plugins de CS2
3. Configurar la base de datos en el archivo de configuración
4. Cargar el plugin en el servidor

## Requisitos

- Servidor CS2 con CSSharp
- Base de datos MySQL/MariaDB