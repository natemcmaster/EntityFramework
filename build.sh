#!/usr/bin/env bash
repoFolder="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd $repoFolder

koreBuildZip="https://github.com/aspnet/KoreBuild/archive/dev.zip"
if [ ! -z $KOREBUILD_ZIP ]; then
    koreBuildZip=$KOREBUILD_ZIP
fi

buildFolder=".build"
buildFile="$buildFolder/KoreBuild.sh"

if test ! -d $buildFolder; then
    echo "Downloading KoreBuild from $koreBuildZip"
    
    tempFolder="/tmp/KoreBuild-$(uuidgen)"    
    mkdir $tempFolder
    
    localZipFile="$tempFolder/korebuild.zip"
    
    retries=6
    until (wget -O $localZipFile $koreBuildZip 2>/dev/null || curl -o $localZipFile --location $koreBuildZip 2>/dev/null)
    do
        echo "Failed to download '$koreBuildZip'"
        if [ "$retries" -le 0 ]; then
            exit 1
        fi
        retries=$((retries - 1))
        echo "Waiting 10 seconds before retrying. Retries left: $retries"
        sleep 10s
    done
    
    unzip -q -d $tempFolder $localZipFile
  
    mkdir $buildFolder
    cp -r $tempFolder/**/build/** $buildFolder
    
    chmod +x $buildFile
    
    # Cleanup
    if test ! -d $tempFolder; then
        rm -rf $tempFolder  
    fi
fi

# If you want to use Microsoft SQL Server in Docker, sign up for the private preview at http://sqlserveronlinux.com/
if [ ! -z "$DOCKER_IMAGE" ];then
    docker_ip="$(docker-machine ip 2>/dev/null || echo "127.0.0.1")"
    if [ ! -z "$DOCKER_LOGIN" ]; then
        docker login -e="$DOCKER_LOGIN" -u "$DOCKER_LOGIN" -p "$DOCKER_PASSWORD" "$DOCKER_REPO"
    fi
    container="$(docker run -d -e SA_PASSWORD=sa -e ACCEPT_EULA=y -p 1433:1433 "$DOCKER_IMAGE")"
    echo "Docker container is $container"
    export Test__SqlServer__DefaultConnection="Server=$docker_ip;User ID=sa;Password=sa;Connect Timeout=30"
    if [ ! -z "$DOCKER_WAIT_FOR_DB" ]; then
        max_wait=60 # two minutes
        until nc -z $docker_ip 1433 ; do
            echo "$(date) - waiting for db server ..."
            sleep 2
            let max_wait-=1
            if [ $max_wait -lt 0 ]; then
                echo "Max wait reached. Exiting 1"
                docker rm --force "$container"
                exit 1
            fi
        done
    fi
fi

$buildFile -r $repoFolder "$@"
exitcode=$?

if [ ! -z "$container" ]; then
    echo "Cleaning up container $container"
    docker rm --force "$container"
fi

exit $exitcode