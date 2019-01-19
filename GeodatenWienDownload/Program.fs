open System
open System.Net
open System.IO
open System.Threading

let basedir = "T:/Geodaten/Wien"
let baseurl = "https://www.wien.gv.at/ma41datenviewer/downloads/ma41/geodaten/lod2_gml";

let createFilename x y = String.Format("{0:000}{1:000}_lod2_gml.zip", x, y)
let createTargetFilename x y = sprintf "%s/%s" basedir (createFilename x y)
let createUrl x y = sprintf "%s/%s" baseurl (createFilename x y)
let createDoesNotExistFilename x y = sprintf "%s/%s" basedir (String.Format("{0:000}{1:000}_does_not_exist", x, y))
let alreadyDownloaded x y = File.Exists(createDoesNotExistFilename x y) || File.Exists(createTargetFilename x y)

[<EntryPoint>]
let main argv =
    if Directory.Exists(basedir) = false then Directory.CreateDirectory(basedir) |> ignore

    let wc = new WebClient()
    for x in [78..136] do
        for y in [62..107] do
            if alreadyDownloaded x y then
                printfn "%s -> cached" (createFilename x y)
            else
                let url = createUrl x y
                let targetfile = createTargetFilename x y
            
                try
                  wc.DownloadFile(url, targetfile)
                  printfn "%s -> downloaded" (createFilename x y)

                with
                | ex -> printfn "%s -> %s" (createFilename x y) ex.Message
                        File.WriteAllText(createDoesNotExistFilename x y, "")
                    
                Thread.Sleep(1000)

    0
    