module AppZooKeeper

open System.Threading.Tasks
open org.apache.zookeeper
open System.Net
open System.Net.NetworkInformation
open System.Net.Sockets

let TARGETS_ZNODE_PATH = "/targets"
let noOpWatcherFunction (event: WatchedEvent) : Task = Task.CompletedTask

type NoOpWatcher() =
    inherit Watcher()
    override _.process(event: WatchedEvent) : Task = Task.CompletedTask

//! ZK Connect string subject to change

//! Address of host subject to change

let getAllLocalIPs() =
    // Get all network interfaces
    NetworkInterface.GetAllNetworkInterfaces()
    |> Array.filter (fun ni -> 
        ni.OperationalStatus = OperationalStatus.Up &&
        ni.NetworkInterfaceType <> NetworkInterfaceType.Loopback)
    |> Array.collect (fun interface' ->
        interface'.GetIPProperties().UnicastAddresses
        |> Seq.map (fun addr -> addr.Address)
        |> Seq.filter (fun addr -> 
            addr.AddressFamily = AddressFamily.InterNetwork || // IPv4
            addr.AddressFamily = AddressFamily.InterNetworkV6) // IPv6
        |> Seq.toArray)

/// Gets only IPv4 addresses
let getIPv4Addresses() =
    getAllLocalIPs()
    |> Array.filter (fun ip -> ip.AddressFamily = AddressFamily.InterNetwork)

/// Gets only IPv6 addresses

/// Gets the primary IP address (usually the first non-loopback IPv4 address)
let getPrimaryIP() =
    getIPv4Addresses()
    |> Array.filter (fun addr -> addr.ToString().StartsWith("192"))
    |> Array.tryHead
    |> Option.defaultWith (fun () -> IPAddress.Parse("127.0.0.1"))

let getCurrentTargetZnodePath hostPort =
    $"{TARGETS_ZNODE_PATH}/{getPrimaryIP()}:{hostPort}"

//! Arguments subject to change
let configureZookeeper (hostPort: string) (zkHost: string) =
    task {
        let zooKeeper = ZooKeeper($"{zkHost}:2181", 3000, NoOpWatcher())
        let! targetListStat = zooKeeper.existsAsync TARGETS_ZNODE_PATH

        if (isNull targetListStat) then
            zooKeeper.createAsync (TARGETS_ZNODE_PATH, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT)
            |> ignore

        let! hostTargetStat = getCurrentTargetZnodePath hostPort |> zooKeeper.existsAsync

        if (isNull hostTargetStat |> not) then
            zooKeeper.deleteAsync ((getCurrentTargetZnodePath hostPort), -1) |> ignore

        zooKeeper.createAsync (
            getCurrentTargetZnodePath hostPort,
            null,
            ZooDefs.Ids.OPEN_ACL_UNSAFE,
            CreateMode.EPHEMERAL
        )
        |> ignore
    }
    |> ignore
