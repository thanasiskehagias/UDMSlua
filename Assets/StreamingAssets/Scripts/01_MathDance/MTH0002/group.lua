-- aliases --
--local LM=require("math")
local UE = CS.UnityEngine
local UT = require('utils')
local Clips = require('animations')
local LF1 = require('functionsGRP'); local LFG = LF1(Group)
local LFO = require('functionsOBJ')
local ROOM = require('functionsROOM');
local LOG = require('logic');
local debug = require('debug')
local math = require('math')

-- variables
local Nagn = Members.Count

local lights = {}

local Ground
local cube1
local sphere1
local empty1

local T0=100
local NormTransDur = 0.35
local TIME = 0

local filePath = ROOM.getScriptPath(Room)..'/out.txt'

function start()
	-- GROUND
	Ground=LFO.makeObject(Room,'Ground','Ground',"plane",UE.Vector3(0, -0.1, 0))
	LFO.setPos(Ground,UE.Vector3(0,-0.2,0))
	LFO.setScale(Ground,UE.Vector3(50,1,50))
	LFO.textureObj(Ground,'textures/ground','checkerboard_2',50,50)

	-- AGENTS
	LFG.toggleIndices(true)
    for i=0,Nagn - 1 do
		LFG.setTurnToMoveDir(i,true)
		LFG.setColorState(i,false)
		LFG.setState(i,1)
		LFG.setTurnToMoveDir(i,true)
		LFG.aniCrossFade(i,Clips[121],NormTransDur,true)
		LFG.trailAttach(i,UE.Vector3(0,1,0),UE.Color.red,0,0.01)
		LFG.trailSetEndColor(i,UE.Color.blue)
		LFG.trailSetEndWidth(i,0.05)
		LFG.trailSetTime(i,50)
	end

	-- LIGHTS
	for i=0,1 do
		lights[i]=LFO.lgtMake(Room,tostring(i),"Light1","spot",UE.Vector3(4*(-1)^i,4,0),UE.Vector3(90,0,0))
		LFO.lgtSetRange(lights[i],60)
		LFO.lgtSetIntensity(lights[i],2)
		LFO.lgtSetSpotAngle(lights[i],155)
		LFO.lgtSetColor(lights[i],UE.Color(1,1,1))
	end
end
--------------------------------------------------------------------------------
local a=4
local b=2
local wx=0.005
local wz=0.015
local xt
local zt
local xti
local zti
local phi
function update()
    TIME = TIME + 1
	UT.writeText(TIME,filePath)
	-- AGENTS
    for i=0,Nagn-1 do
		phi=(i/Nagn)*6.28
		xt =(a+b)*math.cos(wx*TIME)-b*math.cos((a/b+1)*wx*TIME)
		zt =(a+b)*math.sin(wz*TIME)-b*math.sin((a/b+1)*wz*TIME)
		xti= math.cos(phi)*xt+math.sin(phi)*zt
		zti=-math.sin(phi)*xt+math.cos(phi)*zt
		LFG.setPos(i,UE.Vector3(xti,0,zti))
	end
end

function onDestroy()
	UT.closeFile(filePath)
end
--------------------------------------------------------------------------------
--[[ Root motion is off :
local anims = {}
function onElementAnimatorMove(i)
    anims[i]:ApplyBuiltinRootMotion()
end
--]]
--------------------------------------------------------------------------------
--------------------------------------------------------------------------------
