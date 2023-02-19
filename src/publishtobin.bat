REM Publish to $(SolutionDir)bin the build of GUI with exclusions
REM SET SOLDIR=$(SolutionDir)
REM SET PLATFORM=$(Platform)
REM SET CONFNAME=$(ConfigurationName)
REM SET PROJDIR=$(ProjectDir)
		
XCOPY "%PROJDIR%bin\%PLATFORM%\%CONFNAME%" "%SOLDIR%bin\%CONFNAME%" /Y /S /I /EXCLUDE:%~dp0XCOPY_EXCLUDE.txt