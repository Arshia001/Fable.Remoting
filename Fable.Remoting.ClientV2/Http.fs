namespace Fable.Remoting.Client 

open Browser
open Browser.Types 
open Fable.Core

module Http = 

    /// Constructs default values for HttpRequest
    let private defaultRequestConfig : HttpRequest = {
        HttpMethod = GET 
        Url = "/" 
        Headers = [ ]
        RequestBody = Empty 
    }

    /// Creates a GET request to the specified url
    let get (url: string) : HttpRequest = 
        { defaultRequestConfig 
            with Url = url
                 HttpMethod = GET }
    
    /// Creates a POST request to the specified url
    let post (url: string) : HttpRequest = 
        { defaultRequestConfig 
            with Url = url
                 HttpMethod = POST }

    /// Creates a request using the given method and url
    let request method url = 
        { defaultRequestConfig
            with Url = url
                 HttpMethod = method }
    
    /// Appends a request with headers as key-value pairs
    let withHeaders headers (req: HttpRequest) = { req with Headers = headers  }
    
    /// Appends a request with string body content
    let withBody body (req: HttpRequest) = { req with RequestBody = body }

    /// Sends the request to the server and asynchronously returns a response
    let send (req: HttpRequest) =
        Async.FromContinuations <| fun (resolve, _, _) -> 
            let xhr = XMLHttpRequest.Create()
            
            match req.HttpMethod with 
            | GET -> xhr.``open``("GET", req.Url)
            | POST -> xhr.``open``("POST", req.Url)
                
            // set the headers, must be after opening the request
            for (key, value) in req.Headers do 
                xhr.setRequestHeader(key, value)

            xhr.onreadystatechange <- fun _ ->
                match xhr.readyState with
                | ReadyState.Done -> resolve { StatusCode = unbox xhr.status; ResponseBody = xhr.responseText }
                | _ -> ignore() 
         
            match req.RequestBody with 
            | Empty -> xhr.send()
            | RequestBody.Json content -> xhr.send(content)
            | Binary content -> xhr.send(content)

    [<Emit("new Uint8Array($0)")>]
    let internal createUInt8Array (x: obj) : byte[] = jsNative
    
    let sendAndReadBinary (req: HttpRequest) = 
        Async.FromContinuations <| fun (resolve, _, _) -> 
            let xhr = XMLHttpRequest.Create()
            
            match req.HttpMethod with 
            | GET -> xhr.``open``("GET", req.Url)
            | POST -> xhr.``open``("POST", req.Url)
               
            // read response as byte array
            xhr.responseType <- "arraybuffer"

            // set the headers, must be after opening the request
            for (key, value) in req.Headers do 
                xhr.setRequestHeader(key, value)

            xhr.onreadystatechange <- fun _ ->
                match xhr.readyState with
                | ReadyState.Done ->
                    let bytes = createUInt8Array xhr.response
                    resolve (bytes, xhr.status)
                | _ -> 
                    ignore() 
           
            match req.RequestBody with 
            | Empty -> xhr.send()
            | RequestBody.Json content -> xhr.send(content)
            | Binary content -> xhr.send(content) 