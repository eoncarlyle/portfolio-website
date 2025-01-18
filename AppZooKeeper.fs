module AppZooKeeper

open System.Threading.Tasks
open org.apache.zookeeper
open System
open System.Text

let TARGETS_ZNODE_PATH = "/targets"
let noOpWatcherFunction (event: WatchedEvent) : Task = Task.CompletedTask

type NoOpWatcher() =
    inherit Watcher()
    override _.process(event: WatchedEvent) : Task = Task.CompletedTask

let getZooKeeper zkConnectString =
    new ZooKeeper(zkConnectString, 3000, NoOpWatcher())

let getCurrentTargetZnodePath hostAddress hostPort =
    $"{TARGETS_ZNODE_PATH}/{hostAddress}:{hostPort}"

let configureZookeeper (zkConnectString: string) (hostAddress: string) (hostPort: string) =
    task {
        let zooKeeper = getZooKeeper zkConnectString
        let! targetListStat = zooKeeper.existsAsync TARGETS_ZNODE_PATH
        let currentTargetZnodePath = getCurrentTargetZnodePath hostAddress hostPort

        if (isNull targetListStat) then
            zooKeeper.createAsync (TARGETS_ZNODE_PATH, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT)
            |> ignore

        let! hostTargetStat = zooKeeper.existsAsync currentTargetZnodePath

        if (isNull hostTargetStat |> not) then
            zooKeeper.deleteAsync (currentTargetZnodePath, -1) |> ignore

        zooKeeper.createAsync (currentTargetZnodePath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL)
        |> ignore

        zooKeeper.createAsync ("/cacheAge", null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT)
        |> ignore

        zooKeeper.setDataAsync (
            "/cacheAge",
            Encoding.ASCII.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() |> Convert.ToString),
            -1
        )
        |> ignore

    }
    |> ignore
