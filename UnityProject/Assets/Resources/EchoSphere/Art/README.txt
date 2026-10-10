ECHO SPHERE / Unity authored 2.5D art

Migrated from new-desing HEAD 737ef2af4a877e0995db5f92331d536d2f29a934:
Core/player, eight authored Sphere designs, Spherist front and three-quarter
artwork, and boss artwork. The source SVGs are preserved as vector files.
Unsupported SVG filter effects were removed during migration; the path
geometry, strokes, colors and gradients remain. Runtime glow is composed as
a separate SpriteRenderer layer.

sphere-standard.svg and sphere-orbital.svg are authored migration bridge art:
the original Standard visual is composed from separate PNG pieces, and
Orbital's production presentation is a dynamic Canvas renderer. These two
bridge assets are not a claim of final visual parity.

SVG import settings are controlled by Assets/Editor/EchoSphereSvgImportSettings.cs.
