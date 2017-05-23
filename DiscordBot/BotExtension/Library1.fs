module VolumeHandling

open System

//let HandleVolume = fun Text -> 

let ConvertVolume = fun convertedBytes percentage -> ((percentage / 100.0) * (float(convertedBytes)))

let ConvertShortToBytes = fun short -> BitConverter.GetBytes(int16(short))

let ConvertBytesToShort = fun bytes -> BitConverter.ToInt16(bytes)

let ChangeVolumeByPercentage bytes percentage = (ConvertShortToBytes ((ConvertVolume (ConvertBytesToShort bytes)) percentage))