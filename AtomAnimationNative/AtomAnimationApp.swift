import SwiftUI

@main
struct AtomAnimationApp: App {
    @State private var viewModel = AtomViewModel()

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environment(viewModel)
        }

        ImmersiveSpace(id: "AtomSpace") {
            ImmersiveView()
                .environment(viewModel)
        }
        .immersionStyle(selection: .constant(.mixed), in: .mixed)
    }
}
