$fbPath = "c:\Program Files (x86)\FinalBuilder 7\FinalBuilder7.exe"
function Main
{
	&pskill FinalBuilder7.exe
	while($true)
	{
		$cm = "/e /r d:\SupportApps\Download\FTPFeedbacks1.fbz7"
		RunProcess $fbPath $cm
		Start-Sleep -Seconds 420
		&pskill FinalBuilder7.exe
	}
}

function RunProcess( $procName, $argsForProc )
{
	"Run " + $procName + " with " + $argsForProc
	[Diagnostics.Process]::Start( $procName, $argsForProc )
}


Main