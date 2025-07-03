(sudo docker buildx build -f Dockerfile.gateway -t api-gateway .  > api-gateway.out) &

(sudo docker buildx build -f Dockerfile.user -t user . > user.out) &

(sudo docker buildx build -f Dockerfile.book -t book . > book.out) &

(sudo docker buildx build -f Dockerfile.loan -t loan . > loan.out) &

(sudo docker buildx build -f Dockerfile.room -t room . > room.out) &
