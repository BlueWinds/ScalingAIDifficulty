#!/bin/bash
set -o errexit

export BTPATH=../game
dir=$(pwd)

RED='\033[0;31m'
NC='\033[0m' # No Color

if ! command -v nodemon &> /dev/null
then
    echo -e "${RED}nodemon could not be found, compiling once and exiting.${NC}"
    msbuild
    exit
fi

nodemon -x "msbuild && rm -f 'ScalingAIDifficulty' && cd '$BTPATH/Mods/ScalingAIDifficulty' && zip -x ScalingAIDifficulty.log -rq '$PWD/ScalingAIDifficulty.zip' ." -w src/ -e .
