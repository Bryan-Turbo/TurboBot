module VolumeHandling

open System

let private ConvertVolume = fun convertedBytes percentage -> ((percentage / 100.0) * (float(convertedBytes)))

let private ConvertShortToBytes = fun short -> BitConverter.GetBytes(int16(short))

let private ConvertBytesToShort = fun bytes -> BitConverter.ToInt16(bytes)

let ChangeVolumeByPercentage = fun bytes percentage -> (ConvertShortToBytes ((ConvertVolume (ConvertBytesToShort bytes)) percentage))