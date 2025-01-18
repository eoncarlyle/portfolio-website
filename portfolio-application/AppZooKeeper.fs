module AppZooKeeper

open System.Threading.Tasks
open org.apache.zookeeper
open System
open System.Text

let TARGETS_ZNODE_PATH = "/targets"

let COMMIT_ZNODE_PATH = "/portfolioApplicationCommit"

let noOpWatcherFunction (event: WatchedEvent) : Task = Task.CompletedTask

type NoOpWatcher() =
    inherit Watcher()
    override _.process(event: WatchedEvent) : Task = Task.CompletedTask

type RuntimeArgs =
    { ZkConnectString: string
      HostAddress: string
      HostPort: string
      CommitSHA: string
      }

let getZooKeeper zkConnectString =
    new ZooKeeper(zkConnectString, 3000, NoOpWatcher())

let getCurrentTargetZnodePath hostAddress hostPort =
    $"{TARGETS_ZNODE_PATH}/{hostAddress}:{hostPort}"

let configureZookeeper runtimeArgs =
   task {
        let zkConnectString = runtimeArgs.ZkConnectString
        let hostAddress = runtimeArgs.HostAddress
        let hostPort = runtimeArgs.HostPort
        let commitSHA = runtimeArgs.HostAddress
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

        zooKeeper.createAsync (COMMIT_ZNODE_PATH, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT)
        |> ignore
        
        let! commitZnode = zooKeeper.getDataAsync COMMIT_ZNODE_PATH
        
        match commitZnode.Data.ToString() with
        | sha when sha = commitSHA -> ()
        | _ -> zooKeeper.setDataAsync(COMMIT_ZNODE_PATH, Encoding.UTF8.GetBytes(commitSHA), -1 ) |> ignore
        

    }|> ignore
