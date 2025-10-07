# Advanced Statistics Plugin for CS2

[![English](https://img.shields.io/badge/README-English-blue)](README.md)
[![Español](https://img.shields.io/badge/README-Español-red)](README_es.md)

Advanced statistics plugin for Counter-Strike 2 that tracks and stores detailed game data in a MySQL database.

## Features

- Detailed tracking of kills, deaths, and assists by specific weapon
- MySQL database storage
- Customizable configuration
- Option to include or exclude bot events
- Real-time statistics

## Requirements

- Counter-Strike Sharp
- MySQL Server
- .NET 7.0

## Installation

1. Compile the plugin
2. Copy files to the Counter-Strike Sharp plugins folder
3. Configure the database in the configuration file
4. Restart the server

## Configuration

The configuration file is located at `configs/plugins/AdvancedStatistics/AdvancedStatistics.json`:

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

## Database Structure

The plugin automatically creates the `player_stats` table with columns for:

- General statistics (kills, deaths, assists, headshots, etc.)
- Kills by specific weapon (ak47_kills, m4a4_kills, etc.)
- Deaths by specific weapon (ak47_deaths, m4a4_deaths, etc.)
- Assists by specific weapon (ak47_assists, m4a4_assists, etc.)

## Usage

Once installed and configured, the plugin will automatically track game events and store them in the database.

## Development

To compile the plugin:

```bash
dotnet build
```

## Contributing

1. Fork the repository
2. Create a branch for your feature (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.