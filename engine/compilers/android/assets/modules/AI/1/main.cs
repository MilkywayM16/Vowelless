function AI::create( %this )
{	
    AI.Health = 100;
	AI.Score = 0;
	AI.Damage = 0;
	AI.Alive = true;
	AI.CurrentWord = 0;
	AI.Streak = 0;
	AI.WrongStreak = 0;
	AI.Combo = 0;
	AI.LastCorrectTime = 0;
	AI.Attacking = true;
	AI.HitChance = 6;
}

function AI::destroy( %this )
{

}

function AI::readWord(%this)
{
	echo("Starting Attack");
	setRandomSeed(getRealTime());
	%this.Attacking = true;
	%this.tryToAnswer();
}

function AI::incrementScore(%this,%amount)
{
	%this.score = %this.score + %amount;
}

function AI::tryToAnswer(%this)
{
	%roll = getRandom(1,%this.HitChance + getWordDifficulty($GameWordList[AI.CurrentWord]));
	//echo("AI: Trying to attack" SPC %roll);
	if(%roll == 1)
	{
		//echo("AI: Attack" SPC getWordDamage($GameWordList[AI.CurrentWord]));
		AI.incrementScore(getWordValue($GameWordList[AI.CurrentWord]));

		if(stricmp(Game.Mode,"Battle") == 0)
		{
			%this.Damage = %this.Damage + getWordDamage($GameWordList[AI.CurrentWord]);
		}
		else if(stricmp(Game.Mode,"Race") == 0)
		{
			if(AI.Score >= Game.RaceTo)
			{
				Game.endRace();
				return;
			}
		}
		
		AI.CurrentWord++;
	}

	if(%this.Attacking)
		%this.schedule(500,"tryToAnswer");
}

function AI::attackPlayer(%this, %damage)
{
	Player.changeHealth(%damage);
	Game.playHitSound(%damage);
	Game.displayBattleStats();
}

function AI::reset(%this)
{
	%this.Health = 100;
	%this.Score = 0;
	%this.Alive = true;
	%this.CurrentWord = 0;
	%this.Difficulty = 1;
	%this.Streak = 0;
	%this.Combo = 0;
	%this.LastCorrectTime = 0;
	%this.Damage = 0;
}

function AI::changeHealth(%this,%amount)
{
	if((%this.Health + %amount) > 0)
	{
		%this.Health = %this.Health + %amount;
		AI.Health = mFloatLength(AI.Health, 0);
	}
	else
	{
		%this.Health = 0;
		%this.onDeath();
	}
}

function AI::onDeath(%this)
{
	%this.Alive = false;
	//Game.endGame();
}

//-----------------------------------------------------------------------------
