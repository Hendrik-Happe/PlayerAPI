# PlayerAPI
A Linux music player API written in C# 9.0 using MPlayer.

# Requirements

Below are the required dependencies.

## .NET

Install .NET, for example:

```sh
sudo apt-get install dotnet9 dotnet-sdk-9.0
```

Or use the [official installation script](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install).

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

If you also use my web application [TonPi](https://github.com/Hendrik-Happe/TonPi), follow the PostgreSQL setup there.

### PostgreSQL for API Standalone

```sh
sudo apt-get install postgresql
```

Create a user (with a password) and a database. Grant all permissions to this user for this database.

All tables will be created on the first start.

# Configuration

## AppSettings

Edit the `appsettings.json` file.
You can deactivate GPIO input/output by setting the value to `-1`

| Category         | Config                    | Description | 
|-----------------|--------------------------------|------------------------------|
| ConnectionStrings|  Postgres    | Connection string for PostgreSQL  |
|  PlayerConfig    | BasePath |    (absolute) path to the location of the music files      |
|  PlayerConfig    | FifoFile |    (absolute) path for the fifo file for mplayer      |
|    GPIOConfig     |       StatusLED       |      GPIO pin. Indicates that a song is playing       |
|    GPIOConfig     |       PowerLED       |      GPIO pin. Indicates that the system is running       |
|    GPIOConfig     |       NextButton       |      GPIO pin. Button to play next track in a playlist      |
|    GPIOConfig     |       PreviousButton       |      GPIO pin. Button to play previous track in a playlist      |
|    GPIOConfig     |       PauseButton       |      GPIO pin. Button to pause current track      |
|    GPIOConfig     |       PlayButton       |      GPIO pin. Button to continue current track      |
|    GPIOConfig     |       VolumeUpButton       |      GPIO pin. Button increase volume      |
|    GPIOConfig     |       VolumeDownButton       |      GPIO pin. Button to decrease volume      |
|    VolumeConfig     |       MaxVolume       |      Maximum volume. Should be `>= 100`      |
|    VolumeConfig     |       MinVolume       |      Minimum volume. Should be `<= 0` and `MinVolume < MaxVolume`      |
|    VolumeConfig     |       DefaultVolume       |      Volume after startup. Should be `MinVolume < DefaultVolume < MaxVolume`      |
|    VolumeConfig     |       VolumeStep       |      Increase/decrease step       |

# Build

```sh
cd PlayerAPI
<path/to/dotnet>/dotnet publish
```

# Run

```sh
<path/to/dotnet>/dotnet PlayerAPI/bin/Release/net9.0/PlayerAPI.dll --urls http://0.0.0.0:5031
```

## Run as service

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
### Remark

WorkingDirectory should be set to the full path to the dll. Because the appsettings.json will be used at this directory.

Make sure that the system has the permissions to read and write. You can create this file once and run
```sh
sudo mkfifo <path/to/fifo-file>
sudo chmod 777 <path/to/fifo-file>
```

# API

This service provides the following API-Paths

There are 4 states for the player:

| State | Description |
|------------------|-----------------|
| `Stopped` with no selected playlist (default). | 
| `Playing` | A track is selected and is playing. |
| `Paused` | A track is selected but is paused. |
| `Stopped` with selected playlist | A playlist  |

| Path | Input | Description |
|-----------------|--------------------------------|------------------------------|
| `api/player/play/{id}` | `id`: ID of the playlist to play | Selects the playlist with id `id`. A starts playing. Restarts playlist at the beginning if the playlist already stopped. Returns 404 if playlist could not be found otherwise 200 |
| `api/player/pause` |  | Pauses a playing playlist. If no playlist is active or is already paused nothing will happen |
| `api/player/resume` |  | Resumes a paused playlist. If no playlist is active or is already playing nothing will happen |
| `api/player/next` |  | Plays next track of the selected playlist. If no playlist is active nothing will happen. If the playlist is pause it will be resumed on the next track. If there a no more track the playlist will be stopped but the playlist is still selected. |
| `api/player/previous` |  | Plays previous track of the selected playlist. If no playlist is active nothing will happen. If the playlist is pause it will be resumed on the previous track. If its the first track the playlist will be stopped but the playlist is still selected. |
| `api/playlist/` | | Returns all playlist as json |
| `api/playlist/{id}` | `id`: ID of a playlist | Returns playlist with id as json. If a playlist is not found, returns 404 |
| `api/reload` |  | Reloads playlists from the database. If the playlists in the database are edited, you should call this because the playlists are cached. |
| `api/volume/volumeUp` |  | Increases the volume by `volumeStep` until `volumeMax` is reached |
| `api/volume/volumeDown` |  | Decreases the volume by `volumeStep` until `volumeMin` is reached |
| `api/volume/currentVolume` |  | Returns the current volume |

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


