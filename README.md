# PlayerAPI
A Linux music player API developed in C# 9.0 utilizing MPlayer.

# Requirements

The following dependencies are required:

## .NET

To install .NET, you may use:

```sh
sudo apt-get install dotnet9 dotnet-sdk-9.0
```

Alternatively, use the [official installation script](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install):

```sh
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
sudo chmod +x ./dotnet-install.sh
sudo ./dotnet-install.sh --channel 9.0
```

## MPlayer

```sh
sudo apt-get install mplayer
```

## PostgreSQL

If you are also using the [TonPi web application](https://github.com/Hendrik-Happe/TonPi), please refer to its documentation for PostgreSQL setup instructions.

### Standalone PostgreSQL Setup

```sh
sudo apt-get install postgresql
```

Create a user (with a password) and a database. Grant all necessary permissions to this user for the database.

All required tables will be created automatically on the first application start.

# Configuration

## AppSettings

Edit the `appsettings.json` file as needed.
To disable GPIO input/output, set the corresponding value to `-1`.

| Category         | Config                    | Description | 
|------------------|--------------------------|--------------------------------|
| ConnectionStrings| Postgres                 | Connection string for PostgreSQL |
| PlayerConfig     | BasePath                 | Absolute path to the music files directory |
| PlayerConfig     | FifoFile                 | Absolute path to the FIFO file for MPlayer |
| GPIOConfig       | StatusLED                | GPIO pin indicating playback status |
| GPIOConfig       | PowerLED                 | GPIO pin indicating system status |
| GPIOConfig       | NextButton               | GPIO pin for advancing to the next track |
| GPIOConfig       | PreviousButton           | GPIO pin for returning to the previous track |
| GPIOConfig       | PauseButton              | GPIO pin for pausing playback |
| GPIOConfig       | PlayButton               | GPIO pin for resuming playback |
| GPIOConfig       | VolumeUpButton           | GPIO pin for increasing volume |
| GPIOConfig       | VolumeDownButton         | GPIO pin for decreasing volume |
| VolumeConfig     | MaxVolume                | Maximum volume (should be `>= 100`) |
| VolumeConfig     | MinVolume                | Minimum volume (should be `<= 0` and `MinVolume < MaxVolume`) |
| VolumeConfig     | DefaultVolume            | Startup volume (should be `MinVolume < DefaultVolume < MaxVolume`) |
| VolumeConfig     | VolumeStep               | Step size for volume adjustments |

# Build

```sh
cd PlayerAPI
<path/to/dotnet>/dotnet publish
```

# Run

```sh
<path/to/dotnet>/dotnet PlayerAPI/bin/Release/net9.0/PlayerAPI.dll --urls http://0.0.0.0:5031
```

## Running as a Service

```
[Unit]
Description=PlayerAPI
Requires=postgresql.service
[Service]
Type=simple
Restart=always
RestartSec=1
ExecStart=<path/to/dotnet>dotnet <path/to/repo>/PlayerAPI/bin/Release/net9.0/PlayerAPI.dll --urls http://0.0.0.0:5031
WorkingDirectory=<path/to/dll>
KillSignal=SIGINT

[Install]
WantedBy=multi-user.target
```

### Note

Set `WorkingDirectory` to the full path of the DLL, as `appsettings.json` will be loaded from this directory.

Ensure the system has appropriate read and write permissions. To create the FIFO file, execute:

```sh
sudo mkfifo <path/to/fifo-file>
sudo chmod 777 <path/to/fifo-file>
```

# API

The service exposes the following API endpoints:

There are four possible player states:

| State | Description |
|------------------------------|---------------------------------------------|
| `Stopped` with no selected playlist (default) | No playlist is selected or playing |
| `Playing` | A track is selected and currently playing |
| `Paused` | A track is selected but playback is paused |
| `Stopped` with selected playlist | A playlist is selected but not playing |

| Path | Input | Description |
|-----------------------------|-------------------------------|-------------------------------------------------------------|
| `api/player/play/{id}`      | `id`: ID of the playlist to play | Selects the playlist with the specified ID and starts playback. If the playlist was previously stopped, playback restarts from the beginning. Returns 404 if the playlist is not found, otherwise 200. |
| `api/player/pause`          |                               | Pauses the currently playing playlist. If no playlist is active or already paused, no action is taken. |
| `api/player/resume`         |                               | Resumes a paused playlist. If no playlist is active or already playing, no action is taken. |
| `api/player/next`           |                               | Plays the next track in the selected playlist. If no playlist is active, no action is taken. If the playlist is paused, playback resumes on the next track. If there are no more tracks, the playlist is stopped but remains selected. |
| `api/player/previous`       |                               | Plays the previous track in the selected playlist. If no playlist is active, no action is taken. If the playlist is paused, playback resumes on the previous track. If it is the first track, the playlist is stopped but remains selected. |
| `api/playlist/`             |                               | Returns all playlists as JSON. |
| `api/playlist/{id}`         | `id`: ID of a playlist         | Returns the playlist with the specified ID as JSON. Returns 404 if not found. |
| `api/reload`                |                               | Reloads playlists from the database. If playlists in the database are modified, invoke this endpoint to refresh the cache. |
| `api/volume/volumeUp`       |                               | Increases the volume by `volumeStep` until `volumeMax` is reached. |
| `api/volume/volumeDown`     |                               | Decreases the volume by `volumeStep` until `volumeMin` is reached. |
| `api/volume/currentVolume`  |                               | Returns the current volume. |

## Python Client

A [Python client](https://github.com/Hendrik-Happe/PlayerAPI/tree/main/client/openapi_client-1.0.0-py3-none-any.whl) is available for this API.

### Installation

```sh
wget https://github.com/Hendrik-Happe/PlayerAPI/blob/main/client/openapi_client-1.0.0-py3-none-any.whl
pip install openapi_client-1.0.0-py3-none-any.whl
```

### Example Usage

```python
import openapi_client

configuration = openapi_client.Configuration(host="http://0.0.0.0:5031")

with openapi_client.ApiClient(configuration) as api_client:
    api_instance = openapi_client.PlayerApi(api_client)
    api_instance.api_player_play_id_get(1)
```

### GPIO Example

Below is an example GPIO wiring configuration:

| | Pin | Pin | |
| ------ | ------ | ----- | ----|
| RC522 3.3V | 1 | 2 | Amplifier 5V |
| - | 3 | 4 | - |
| - | 5 | 6 | Amplifier Ground |
| - | 7 | 8 | - |
| RC522 Ground | 9 | 10 | - |
| - | 11 | 12 | Status LED + (GPIO 18) |
| Next Button + (GPIO 27) | 13 | 14 | Status LED Ground |
| - | 15 | 16 | - |
| - | 17 | 18 | - |
| RC522 MOSI | 19 | 20 | Next Button Ground |
| RC522 MISO | 21 | 22 | RC522 RST |
| RC522 SCK | 23 | 24 | RC522 SDA |
| Power LED Ground | 25 | 26 | - |
| - | 27 | 28 | - |
| Power LED + (GPIO 5) | 29 | 30 | Volume Up Ground |
| - | 31 | 32 | Volume Up + (GPIO 12) |
| - | 33 | 34 | Volume Down Ground |
| - | 35 | 36 | Volume Down + (GPIO 16) |
| Previous Button + (GPIO 26) | 37 | 38 | - |
| Previous Button Ground | 39 | 40 | - |