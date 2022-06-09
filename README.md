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

Under this assumption, we propose a general space manipulation algorithm for RDW as follows. As shown in Figure \ref{fig:algo_step}(a), \ref{fig:algo_step}(b), and line \ref{alg:step1-1}--\ref{alg:step1-2} in Algorithm \ref{alg:nav}, this method first set the room the user starts with to the center of the real space and then initializes the rooms by compressing all neighboring rooms of the initial room to the inside of the real space.

Afterward, this method proceeds with the restore-compression phases whenever the user visits a new room (Figure \ref{fig:algo_step}(c) and line \ref{alg:step2-1}--\ref{alg:step2-2} in Algorithm \ref{alg:nav}). As shown in Figure \ref{fig:algo_step}(d), and line \ref{alg:step3-1}--\ref{alg:step3-2} in Algorithm \ref{alg:nav}, it calculates the scale from the current dimension of the room to its original dimension and the translation from the current center position of the room to the center of real space. The restore phase is performed if the calculated scale is not $\mathbf{I}$ or translation factor is not $\mathbf{0}$. In the restore phase, it first calculates the position to which each wall should be moved by combining the scale and translation obtained above (line \ref{alg:step4} in Algorithm \ref{alg:restore}). Next, it repeatedly moves the walls outside the user's field of view by applying the detection threshold of wall movement gain, which we define as a new type of gain to move the wall, until each wall reaches the corresponding position (Figure \ref{fig:algo_step}(e) and line \ref{alg:step5} in Algorithm \ref{alg:restore}). After the restore phase is completed, the compression phase compresses all neighboring rooms to the current user's room into the real space. For each wall in the neighboring room, the algorithm first finds the side of $R$ parallel and nearest to the wall. Then, it moves the wall towards that side of $R$ (Figure \ref{fig:algo_step}(f) and line \ref{alg:step6-1}--\ref{alg:step6-2} in Algorithm \ref{alg:compression}). As a result, it is possible to walk the virtual space where the infinite number of rooms are interconnected without colliding with real space by repeating these restoration-compression phases.

<p align="center">
  <img 
    width="100%"
    src="/method.png"
  >
</p>

# Demo
https://www.youtube.com/watch?v=qC14Agw4A0M
