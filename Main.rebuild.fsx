open System.IO
open System

type Dictionary = Map<byte, string>
type ReversedDictionary = Map<string, byte>

let joinWith (separator: string) (iterable: seq<char>) = String.Join(separator, iterable)

// bytes

let readFileAsBytes filename =
  File.ReadAllBytes(filename)

let intToBinary (x: int) =
  Convert.ToString(x, 2)

let padWithZeros length (x: string) =
  x.PadLeft(length, '0')

let byteToBinaryOfWordLength wordLength =
  int >> intToBinary >> (padWithZeros wordLength)

let encode (dictionary: Map<byte, string>) (bytes: byte[]) =
  bytes
    |> Array.map (fun byte -> Map.find byte dictionary)
    |> String.concat ""


let simpleDictionary =
  let bytes = seq { 0 .. 255 } |> Seq.map byte
  let encoded = Seq.map (byteToBinaryOfWordLength 8) bytes
  let pairs = Seq.zip bytes encoded

  Map.ofSeq pairs


let reverse (dictionary: Dictionary): ReversedDictionary =
      Map.fold (fun dict key value -> dict.Add(value,key)) Map.empty dictionary


// binary

let readFileAsBinary filename =
  File.ReadAllText filename

let binaryToByte (b: System.String) =
  Convert.ToInt32(string b, 2)

let findFirstSymbolFromDictionary (dictionary: ReversedDictionary) (symbols: char[]): byte * char[] =
  let mutable found = false
  let mutable result = 0 |> byte
  let mutable numOfDigits = 1

  while not found do
    let key = Array.take numOfDigits symbols |> joinWith ""
    match Map.tryFind key dictionary with
    | Some value -> do
      result <- value
      found <- true
    | None -> do
      numOfDigits <- numOfDigits + 1

  result, symbols.[numOfDigits..]

let decode (dictionary: ReversedDictionary) (s: string): byte[] =
  let mutable chars: char[] = s.ToCharArray()
  let mutable complete = false
  let mutable result: byte[] = [||]
  let mutable i = 0

  while not complete do
    let byte, newChars = findFirstSymbolFromDictionary dictionary chars

    result <- Array.append result [| byte |]

    if Array.length newChars > 0 then do
      chars <- newChars
    else do
      complete <- true

    printf "%A\n" i
    i <- i + 1

  result


let writeStringTo filename s =
  File.WriteAllText(filename, s)

let writeBytesTo filename bytes =
  File.WriteAllBytes(filename, bytes)



// distribution

type Distribution = Map<int, int>

let incrementCountOf map (value: int) =
  let maybePreviousOccurences = Map.tryFind value map

  let newOccurences =
    match maybePreviousOccurences with
      | Some x -> x + 1
      | None -> 1

  Map.add value newOccurences map

let distribution (values: int[]): Distribution =
    Array.fold incrementCountOf Map.empty values


// MAIN

"Main.fsx"
  |> readFileAsBytes
  |> encode simpleDictionary
  |> writeStringTo "Main.fsx.01"

"Main.fsx.01"
  |> readFileAsBinary
  |> decode (reverse simpleDictionary)
  |> writeBytesTo "Main.rebuild.fsx"