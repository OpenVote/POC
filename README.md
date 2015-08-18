<link rel="image_src" href="/myimage.jpg"/>
# POC
Proof of concept for OpenVote Platform
<img src='OpenVote.io_Platform.png' />
	
<h2>Limitations in POC </h2>
<ul>
	<li>Its all 1 chain, which means there is a scaling issue in the POC.. The fix will be to allow multiple parallel chains to run, and then each chain can be bundled and linked into a single final vote or tally</li>
	<li>Each chain needs its own genesis node</li>
</ul>
<h2>Design chooses</h2>
<ul>
	<li>The Secret is either randomly created or user entered.. If forgotten there is no way for a voter to truely validate there ballot.. they can see the results but the secret signature is a way for a voter to indpendently verify there vote has not been tamptered with</li>
	<li>No editing, once submitted its signed, private keys are lost on purpose, no editing</li>
	<li>There should be at least 1 close ballot at the end of each chain to "seal" the last ballot given that the chain approach protects the previous ballot not the current one</li>
	<li>Ellyptical Curve encryptions are significantly harder to break.. However, which one is used is kind of up for grabs.</li>
	<li>Guids for Ballot IDs mean that they are preditable but still short enough (as few as 22 characters) that people could almost, maybe, type them in successfully</li>
</ul>
<h2>Community request</h2>
<ul>
	<li>Poke holes in the methodology. This is the core of the concept.</li>
	<li>Help determin how to bring third party observation into the model</li>
	<li>Help build out the spec for a Ballot, Ballot Chain, Vote and Bundle of ballots</li>
</ul>
