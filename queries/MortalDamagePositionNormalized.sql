select
PositionX AS GridPositionX, 
PositionY AS GridPositionY,
PositionZ AS GridPositionZ, 
 SUM(Mortal) AS TotalDamageInPosition,
 SUM(Mortal) / MAX(SUM(Mortal)) OVER() AS NormalizedDamage
 FROM Hit where Mortal = 1
 GROUP BY GridPositionX, GridPositionY, GridPositionZ;