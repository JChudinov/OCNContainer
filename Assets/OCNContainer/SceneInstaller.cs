
using OCNContainer.InternalData;

namespace OCNContainer
{
    public abstract class SceneInstaller
    {
        private Container _sceneContainer;

        protected abstract void SceneSetup();
    }
}
///TODO: create global settings:
/// 1) internal container lookup for registration in terms of deep profiling
///TODO:
//Full roadmap:
///SceneInstaller:
///---RegisterFactory<TFactory> -> Factory would be in global scope
///---RegisterService<TFacade> -> Facade of service would be singleton and accessible in scene scope
///TODO: that's it for scene installer? whatever should else be there?
///
/// Factory:
/// Should implement Factory<TFacade> abstract class, where TFacade is the Facade, registered in Installer of createable by factory object
/// ---Create<T> returns Facade of Instantiated object
