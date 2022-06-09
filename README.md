# Infinite-Room-Manipulation
We present a change-blindness based redirected walking algorithm that allows a user to explore on foot a virtual indoor environment consisting of an infinite number of rooms while at the same time ensuring collision-free walking for the user in real space. This method uses change blindness to scale and translate the room without the user's awareness by moving the wall while the user is not looking. Consequently, the virtual room containing the current user always exists in the valid real space. We measured the detection threshold for whether the user recognizes the movement of the wall outside the field of view. Then, we used the measured detection threshold to determine the amount of changing the dimension of the room by moving that wall. We conducted a live-user experiment to navigate the same virtual environment using the proposed method and other existing methods. As a result, users reported higher usability, presence, and immersion when using the proposed method while showing reduced motion sickness compared to other methods. Hence, our approach can be used to implement applications to allow users to explore an infinitely large virtual indoor environment such as virtual museum and virtual model house while simultaneously walking in a small real space, giving users a more realistic experience.

<p align="center">
  <img 
    width="80%"
    src="/teaser_thumbnail.png"
  >
</p>

# Method
In this study, we propose a novel approach to space manipulation in RDW by moving the wall outside the user's field of view wherever the user is located in an arbitrary room in virtual indoor environments. Typically, an indoor environment is composed of rooms and corridors connecting them. Furthermore, if we consider the corridors are just another type of room, the general indoor environment can be represented as a set of the infinite number of adjacent rooms. Hence, we can assume the virtual indoor environment and the real space as Figure \ref{fig:env}. We assume the real space $R$ is a rectangle, and the virtual space $V$ consists of $n$ rooms, each of which is a rectangle with a size that can be contained entirely within the real space. Lastly, each room could be connected to another adjacent room by a door.

<p align="center">
  <img 
    width="100%"
    src="/method.png"
  >
</p>

# Demo
https://www.youtube.com/watch?v=qC14Agw4A0M
