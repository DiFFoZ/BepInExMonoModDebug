# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] 2024-04-10
### Changed
- Patch of `StackFrame.AddFrames` to make it compatible with other mods (like LethalLib).
### Fixed
- MonoMod patches was breaking IL offset.

## [1.0.1] 2024-04-06
### Changed
- Wrap patching with try/catch if some mod already patching the same method.

## [1.0.0] 2024-04-06
### Added
- IL offset printing on exception.
### Changed
- Dumping patches to make them really fast.

## [0.0.3] 2024-03-21
### Fixed
- Exception that thrown if `Dump.Save` was disabled

## [0.0.2] 2024-03-21
### Changed
- Updated README.md to show what it does

## [0.0.1] 2024-03-21
### Added
- Project files
