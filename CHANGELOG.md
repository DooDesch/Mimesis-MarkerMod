# Changelog

All notable changes to MarkerMod are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/), and this
project adheres to Semantic Versioning.

## [1.4.2] - 2026-06-17

### Changed
- Started maintaining a full changelog that is now published on GitHub, Thunderstore and Nexus. No gameplay changes compared to 1.4.1.

## [1.4.1] - 2026-06-16

### Changed
- Updated the MimicAPI dependency to 0.3.0 to match the current game build.
- Refreshed the README for the June 2026 standard (accurate configuration, dependencies and badges).

## [1.4.0] - 2026-06-15

### Fixed
- Compatibility with the Mimesis 0.3.0 game update and MelonLoader 0.7.3 (updated the MimicAPI reference for the new game build).

### Changed
- Refreshed the README and project documentation with new badges and title format.

## [1.3.1] - 2025-11-16

### Added
- Paintball colour-change feature: paintballs cycle through colours, with the chosen colour applied to both the splatter material and the floor decals for consistent markers.

### Changed
- Reworked colour handling into dedicated material and decal colour services for more reliable colouring.

## [1.3.0] - 2025-11-15

### Added
- MimicAPI integration. Paintball and decal persistence internals are now accessed through MimicAPI's reflection helpers for better compatibility across game updates.

### Changed
- Improved decal persistence performance and compatibility.
- Tidied up the configuration namespace and preference descriptions.

## [1.2.0] - 2025-11-15

### Fixed
- Release packaging and Thunderstore upload fixes, including correct README (UTF-8) handling and the post-build copy step now only running on Windows.

## [1.1.0] - 2025-11-15

### Added
- Initial release. Makes paintballs work as practical markers in dungeons: paintballs create puddles and leave footprints that no longer vanish too quickly, so you can mark your way through the maps.
- Configurable marker lifetime plus toggles for keeping footprints and puddles.
- Optional infinite paintballs.
- Automated build-and-release pipeline and Thunderstore packaging.
