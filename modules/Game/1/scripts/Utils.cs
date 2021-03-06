function getASCIIValue(%letter)
{
	%testStr = "0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_'abcdefghijklmnopqrstuvwxyz";
	if(strpos(%testStr, %letter) != -1)
		%val = strpos(%testStr, %letter) + 17;
		
	return %val; 
}

function removeVowels(%word)
{
	%word = stripChars(%word,"A");
	%word = stripChars(%word,"E");
	%word = stripChars(%word,"I");
	%word = stripChars(%word,"O");
	%word = stripChars(%word,"U");
	//%word = stripChars(%word,"Y");
	%word = stripChars(%word,"a");
	%word = stripChars(%word,"e");
	%word = stripChars(%word,"i");
	%word = stripChars(%word,"o");
	%word = stripChars(%word,"u");
	//%word = stripChars(%word,"y");
	//%word = stripTrailingSpaces(%word);
	
	//echo("Word:" SPC %word);
	
	return %word;
}

function removeVowelsAndPutUnderScore(%word)
{
	//echo("Removing Vowels from:" SPC %word);
	for(%i = 0; %i < 10; %i++)
	{
		%charIndex = strpos(%word,$Vowels[%i]);
		if(%charIndex != -1)
		{
			%firstHalf = getSubStr(%word, 0 , %charIndex);
			if(%charIndex != (strlen(%word)-1))
			{
				%secondHalf = getSubStr(%word, %charIndex + 1, (strlen(%word)-(%charIndex+1)));
				%word = %firstHalf @ "_" @  %secondHalf;
			}
			else
			{
				%word = %firstHalf @ "_";
			}
			
			//echo("First:" SPC %firstHalf SPC "Second:" SPC %secondHalf);
			%i--;
		}
	}

	//%word = stripTrailingSpaces(%word);
	
	//echo("Finished Word:" SPC %word);
	
	return %word;
}

function getNumberOfVowels(%word)
{
	%vowelCount = 0;

	for(%i = 0; %i < 10; %i++)
	{
		%charIndex = strpos(%word,$Vowels[%i]);
		if(%charIndex != -1)
		{
			%firstHalf = getSubStr(%word, 0 , %charIndex);
			%secondHalf = getSubStr(%word, %charIndex + 1, (strlen(%word)-(%charIndex+1)));
			%word = %firstHalf @ "_" @  %secondHalf;
			%vowelCount++;
			%i--;
		}
	}
	
	//echo("Word:" SPC %word SPC "Vowel Count:" SPC %vowelCount);
	
	return %vowelCount;
}

function getWordDifficulty(%word)
{
	%vowelCount = getNumberOfVowels(%word);
	%wordDiff = 1;
	
	if(%vowelCount <= 2)
	{
		%wordDiff = 1;
	}
	else if(%vowelCount <= 4)
	{
		%wordDiff = 2;
	}
	else if(%vowelCount <= 6)
	{
		%wordDiff = 3;
	}
	else
	{
		%wordDiff = 4;
	}
	
	//echo("Word:" SPC %word SPC "Word Difficulty:" SPC %wordDiff);
	
	return %wordDiff;
}

function shuffleGameWordList()
{
	//setRandomSeed(getRealTime());	//Randomize Each Time
	setRandomSeed(1);				//Same Sequence each time
	%size = Game.GameWordListSize;

	if(%size > 1)
	{
		for(%i = 0; %i < (%size - 1); %i++)
		{
			%j = %i + getRandom(0,((%size-1)-%i));
			%temp = $GameWordList[%j];
			$GameWordList[%j] = $GameWordList[%i];
			//echo("Swapping" SPC $GameWordList[%i] SPC "with" SPC %temp);
			$GameWordList[%i] = %temp;
		}
	}
	echo("Shuffled Array");
}

function readWordsFile()
{
	%file = new FileObject();
	
	if(%file.openForRead("./data/words.txt"))
	{
		%x=1;
		while(!%file.isEof())
		{
			%line = %file.readLine();
			//echo("line" @ %x @ " = " @ %line);
			$FullWordList[%x-1] = %line;
			Game.FullWordListSize++;
			%x++;
		}
	}
	else
	{
		error("CANNOT OPEN FOR READ");
	}
	
	%file.close();
	%file.delete();
	
	echo("Done Reading File. Words Found:" SPC Game.FullWordListSize);
	
	setupGameWordList();
}

function setupGameWordList()
{
	for(%i = 0; %i < Game.FullWordListSize; %i++)
	{
		if(getWordDifficulty($FullWordList[%i]) == Player.Difficulty)
		{
			$GameWordList[Game.GameWordListSize] = $FullWordList[%i];
			Game.GameWordListSize++;
		}
	}
	
	shuffleGameWordList();
	
	echo("Done Game List. Words:" SPC Game.GameWordListSize);
	
	setupVowellessWordList();
}

function setupVowellessWordList()
{
	for(%i = 0; %i < Game.GameWordListSize; %i++)
	{
		if(Player.Difficulty == 4)
		{
			$VowellessList[%i] = removeVowels($GameWordList[%i]);
		}
		else
		{
			$VowellessList[%i] = removeVowelsAndPutUnderScore($GameWordList[%i]);
		}
	}
}

function isLetterAVowel(%letter)
{
	%flag = false;
	
	for(%i = 0; %i < 10; %i++)
	{
		if(stricmp(%letter,$Vowels[%i]) == 0)
		{
			%flag = true;
		}
	}
	
	return %flag;
}

function setupVowels()
{
	$Vowels[0] = "A";
	$Vowels[1] = "a";
	$Vowels[2] = "E";
	$Vowels[3] = "e";
	$Vowels[4] = "I";
	$Vowels[5] = "i";
	$Vowels[6] = "O";
	$Vowels[7] = "o";
	$Vowels[8] = "U";
	$Vowels[9] = "u";
}

function getNumberOfVisibleLetters(%word)
{
	return (strlen(%word) - getNumberOfVowels(%word));
}

function getWordValue(%word)
{
	%baseScore = 100;
	%baseComboScore = 100;
	%baseStreakScore = 25;
	%score = (3/getNumberOfVisibleLetters(%word))*%baseScore + Player.Combo*%baseComboScore + Player.Steak*%baseStreakScore;
	
	return %score;
}

function getWordDamage(%word)
{
	%baseDamage = 10;
	%damage = %baseDamage*(getNumberOfVowels(%word)/getNumberOfVisibleLetters(%word));
	%damage = mFloatLength(%damage, 2);
	return %damage;
}