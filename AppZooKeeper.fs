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

//! ZK Connect string subject to change
let zooKeeper = new ZooKeeper("localhost:2181", 3000, NoOpWatcher())

//! Address of host subject to change
let getCurrentTargetZnodePath hostPort =
    $"{TARGETS_ZNODE_PATH}/localhost:{hostPort}"

//! Arguments subject to change
let configureZookeeper (hostPort: string) =
    task {
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
