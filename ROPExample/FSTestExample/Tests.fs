namespace FSTestExample

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type ``User authentication tests`` () =

    [<TestMethod>]
    member this.TestMethodPassing () =
        Assert.IsTrue(true);
