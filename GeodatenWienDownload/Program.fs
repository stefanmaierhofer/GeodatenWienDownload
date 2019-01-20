open System
open System.Net
open System.IO
open System.Threading
open Flurl

let targetdir = "T:/Geodaten/Wien"
let baseurl = "https://www.wien.gv.at/ma41datenviewer/downloads/ma41/geodaten"

type Item = { Filename : string; FilenameFull : string; Url : string}
type SeqType = Fine | Coarse
type Dataset = { Type : SeqType; Dir : string; Name : string}

let datasets = [|
    // Luftaufnahmen
    { Type = Coarse;    Dir = "op_img";         Name = "op" }
    { Type = Coarse;    Dir = "op_img";         Name = "op_2017" }
    { Type = Coarse;    Dir = "op_img";         Name = "op_2016" }
    { Type = Coarse;    Dir = "op_img";         Name = "op_2015" }
    { Type = Coarse;    Dir = "op_img";         Name = "op2014" }
    { Type = Coarse;    Dir = "lb_img";         Name = "lb1956" }
    { Type = Coarse;    Dir = "lb_img";         Name = "lb1938" }
    // MZK Vektordaten
    { Type = Fine;      Dir = "vmzk_dxf";       Name = "mzk" }
    // MZK Rasterdaten
    { Type = Coarse;    Dir = "vmzk_img";       Name = "mzkfarb" }
    // Flächen-MZK Vektordaten
    { Type = Coarse;    Dir = "fmzk_shp";       Name = "fmzk" }
    // Flächen-MZK Rasterdaten
    { Type = Coarse;    Dir = "fmzk_img";       Name = "fmzkrt" }
    // Baukörpermodell (LOD1)
    { Type = Fine;      Dir = "lod1_dxf";       Name = "lod1" }
    { Type = Fine;      Dir = "fmzk_bkm";       Name = "bkm" }
    // Generalisiertes Dachmodell (LOD2)
    { Type = Fine;      Dir = "lod2_gml";       Name = "lod2_gml" }
    { Type = Fine;      Dir = "lod2_dxf";       Name = "lod2_dxf" }
    // Geländemodell (DGM)
    { Type = Coarse;    Dir = "dgm_iso_shp";    Name = "dgm_iso" }
    { Type = Coarse;    Dir = "dgm_tif";        Name = "dgm_tif" }
    { Type = Coarse;    Dir = "dgm_asc";        Name = "dgm_asc" }
    { Type = Coarse;    Dir = "dgm_tin_dxf";    Name = "dgm_tin" }
    { Type = Coarse;    Dir = "dgm_bk_shp";     Name = "dgm_bk" }
    // Oberflächenmodell (DOM)
    { Type = Coarse;    Dir = "dom_asc";        Name = "dom_asc" }
    { Type = Coarse;    Dir = "dom_tif";        Name = "dom_tif" }
    |]
    
let createItem prefix f = { Filename = f; FilenameFull = Path.Combine(targetdir, prefix, f); Url = Url.Combine(baseurl, prefix, f) }
let seqFine dir s () = seq {
    for x in [78..136] do
        for y in [62..107] do
            yield createItem dir (String.Format("{0:000}{1:000}_{2}.zip", x, y, s))
    }
let seqCoarse dir s () = seq {
    for x in [15..58] do
        for y in [1..4] do
            yield createItem dir (String.Format("{0}_{1}_{2}.zip", x, y, s))
    }
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

    for x in datasets do
        let seq = match x.Type with | Fine -> seqFine | Coarse -> seqCoarse
        download x.Dir (seq x.Dir x.Name)

    0
 