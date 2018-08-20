
set H=R:\KSP_1.4.3_dev
echo %H%
cd

copy /Y "ResonantOrbitCalculator\bin\Debug\ResonantOrbitCalculator.dll" "GameData\ResonantOrbitCalculator\Plugins"
copy /Y ResonantOrbitCalculator.version GameData\ResonantOrbitCalculator

cd GameData
xcopy /y /s /I ResonantOrbitCalculator "%H%\GameData\ResonantOrbitCalculator"
