packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -target:"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\MSTest.exe" -targetargs:"/noisolation /testcontainer:LDAPLibraryUnitTest\bin\Debug\LDAPLibraryUnitTest.dll" -filter:+[*]* -output:"coverageResult.xml"
packages\ReportGenerator.2.4.3.0\tools\ReportGenerator.exe -reports:"coverageResult.xml" -targetdir:"LDAPLibraryCodeCoverage" 
pause
