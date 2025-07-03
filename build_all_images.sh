(sudo docker buildx build -f Dockerfile.gateway -t api-gateway .  2> api-gateway.out && sudo docker tag api-gateway roccobalocco/api-gateway) &

(sudo docker buildx build -f Dockerfile.user -t user . 2> user.out && sudo docker tag user roccobalocco/user) &

(sudo docker buildx build -f Dockerfile.book -t book . 2> book.out && sudo docker tag book roccobalocco/book) &

(sudo docker buildx build -f Dockerfile.loan -t loan . 2> loan.out && sudo docker tag loan roccobalocco/loan) &

(sudo docker buildx build -f Dockerfile.room -t room . 2> room.out && sudo docker tag room roccobalocco/room) &




