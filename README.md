Welcome to Scott's submission for the coding take home piece.

I have provided Eileen with the api key to be slotted in Program.cs line 86 (mostly because github gave me a security alert because obvs you wouldn't hard code the api key in. You'd use some key store for it.)
I have provided a launchSettings.json which can be modified with command line args to demonstrate the project. I have done it such that you call it "SmokeBallTakeHome.exe www.smokeball.com.au conveyancing software" where the first arg is the url you're hunting for, and any following args are the search terms you wish to provide. It expects at least 1.
Notes about implementation:
Where I've private method'ed out some methods in program.cs, I would inject them as components/interfaces.
Api key and the cx should be gotten from some form of key vault.
