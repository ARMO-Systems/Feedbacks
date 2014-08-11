setwd('c:/temp')
data = read.table("OS.txt", header = TRUE, sep=";", na.strings = "NA")
attach(data)
counts = nrow(data)

osVersions = table(OSVersion)
pie(osVersions, col=rainbow(6), main = "Timex User's OS all timex versions")
osVersionF = factor(OSVersion)
freqOS = table(osVersionF)
cbind(freqOS, freqOS/counts)
counts

versionF=factor(VersionInfo)
library(gmodels)
joint = CrossTable(osVersionF, versionF, prop.chisq=FALSE)
barplot(joint$prop.col, beside=FALSE, col=rainbow(6), ylab='Windows Frequency', xlab='Timex Versions', legend = rownames(osVersions) )


setwd('c:/temp')
data = read.table("DB.txt", header = TRUE, sep=";", na.strings = "NA")
attach(data)
emps = table(c( "<30", "30-100", "100-1000", ">1000")[findInterval(EmployerXPO , c(-Inf, 30, 100, 1000, Inf) ) ])
pie(emps, col=rainbow(6), main = paste("Timex Emps count:", length(na.omit(EmployerXPO))))

opers = table(TimexOperator2XPO)
pie(opers, col=rainbow(6), main = paste("Timex Operators count:", length(na.omit(TimexOperator2XPO))))