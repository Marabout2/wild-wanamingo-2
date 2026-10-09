# Wild Wanamingo

**Wild Wanamingo** is a Fallout-themed fork of [Space Station 14](https://github.com/space-wizards/space-station-14), the remake of SS13 that runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), an engine written in C#.

It is a rebase of [Misfits: Nuclear Wasteland](https://github.com/Misfit-Sanctuary/nuclear-14) onto current Space Station 14, which itself grew out of [Nuclear 14](https://github.com/Vault-Overseers/nuclear-14), the first Fallout fork of SS14, created by Peptide90 in 2022 with contributions from the community.

## About the project

Wild Wanamingo combines:

* Content and systems from Nuclear 14 and Misfits
* Sprites from [Mojave Sun 13](https://github.com/Mojave-Sun/mojave-sun-13) and other Fallout 13 builds
* The current Space Station 14 codebase, kept close to upstream so it can keep receiving its updates

The codebase is licensed under AGPLv3 (see [License](#license)).

## Links

* Source code: <https://github.com/fallout14/wild-wanamingo-2>
* SS14 documentation: <https://docs.spacestation14.com/>

## Contributing

See [CONTRIBUTING_WW.md](./CONTRIBUTING_WW.md) for how this repository is organized: where new and ported code goes, ID prefixes, attribution headers and change markers.

## Building

### Requirements

* Git
* Python 3
* .NET SDK (version in `global.json`)

### Steps

1. Clone this repo:
```shell
git clone https://github.com/fallout14/wild-wanamingo-2.git
```
2. Go to the project folder and run `RUN_THIS.py` to initialize the submodules and load the engine:
```shell
cd wild-wanamingo-2
python RUN_THIS.py
```
3. Compile the solution:

Build the server using `dotnet build`.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License

Detailed information about code and asset licensing is in [LEGAL.md](./LEGAL.md). Key provisions:

* Code: AGPLv3 for Wild Wanamingo, Misfits and Nuclear 14 code; Space Station 14 code keeps its MIT license
* Assets: individual licenses (check each asset's meta.json)
* If you run a modified server, AGPLv3 requires offering its source code to players
