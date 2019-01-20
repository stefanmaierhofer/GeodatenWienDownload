open System
open System.Net
open System.IO
open System.Threading
open Flurl

let targetdir = "T:/Geodaten/Wien"
let baseurl = "https://www.wien.gv.at/ma41datenviewer/downloads/ma41/geodaten"

type Item = { Filename : string; FilenameFull : string; Url : string}

let createItem prefix f = { Filename = f; FilenameFull = Path.Combine(targetdir, prefix, f); Url = Url.Combine(baseurl, prefix, f) }

let seqLod2Gml () = seq {
    for x in [78..136] do
        for y in [62..107] do
            yield createItem "lod2_gml" (String.Format("{0:000}{1:000}_lod2_gml.zip", x, y))
    }
    
let private seqMeta dir s () = seq {
    for x in [15..58] do
        for y in [1..4] do
            yield createItem dir (String.Format("{0}_{1}_{2}.zip", x, y, s))
    }
let seqDgmTif = seqMeta "dgm_tif" "dgm_tif"
let seqFmzkShp = seqMeta "fmzk_shp" "fmzk"

let ensureDirectory name = if Directory.Exists(name) = false then Directory.CreateDirectory(name) |> ignore

let download prefix items =
    let wc = new WebClient()
    ensureDirectory targetdir
    ensureDirectory (Path.Combine(targetdir, prefix))
    for item in items () do
        let targetNA = item.FilenameFull + ".does_not_exist"

        if File.Exists(item.FilenameFull) || File.Exists(targetNA) then
            printfn "%s -> cached" item.Filename
        else
            try
                wc.DownloadFile(item.Url, item.FilenameFull)
                printfn "%s -> downloaded" item.Filename

            with
            | ex -> printfn "%s -> %s" item.Filename ex.Message
                    File.WriteAllText(targetNA, "")
                    
            Thread.Sleep(500)

[<EntryPoint>]
let main argv =

    download "lod2_gml" seqLod2Gml
    download "dgm_tif" seqDgmTif
    download "fmzk_shp" seqFmzkShp

    0
    