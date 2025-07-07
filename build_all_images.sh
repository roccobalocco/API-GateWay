(sudo docker buildx build -f Dockerfile.gateway -t api-gateway .  2> api-gateway.out && sudo docker tag api-gateway roccobalocco/api-gateway 2> api-gateway-tag.out && sudo docker push roccobalocco/api-gateway ) &

(sudo docker buildx build -f Dockerfile.user -t user . 2> user.out && sudo docker tag user roccobalocco/user 2> user-tag.out && sudo docker push roccobalocco/user) &

(sudo docker buildx build -f Dockerfile.book -t book . 2> book.out && sudo docker tag book roccobalocco/book 2> book-tag.out && sudo docker push roccobalocco/book) &

(sudo docker buildx build -f Dockerfile.loan -t loan . 2> loan.out && sudo docker tag loan roccobalocco/loan 2> loan-tag.out && sudo docker push roccobalocco/loan) &

(sudo docker buildx build -f Dockerfile.room -t room . 2> room.out && sudo docker tag room roccobalocco/room 2> room-tag.out && sudo docker push roccobalocco/room) &




